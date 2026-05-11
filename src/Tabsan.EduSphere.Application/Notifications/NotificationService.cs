using Microsoft.Extensions.Caching.Memory;
using Tabsan.EduSphere.Application.DTOs.Notifications;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Application.Notifications;

/// <summary>
/// Orchestrates notification composition, fan-out dispatch, and read-state management.
/// Business rules enforced here:
///   - Fan-out: one NotificationRecipient row is created per recipient on dispatch.
///   - Only active notifications appear in user inboxes.
///   - MarkRead is idempotent — calling it twice is safe.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repo;
    private readonly INotificationFanoutQueue? _fanoutQueue;
    private readonly IMemoryCache _cache;

    private const int DeferredFanoutThreshold = 250;
    private static readonly TimeSpan InboxCacheTtl = TimeSpan.FromSeconds(10);
    private static readonly TimeSpan BadgeCacheTtl = TimeSpan.FromSeconds(8);
    private static int _cacheVersion;

    public NotificationService(INotificationRepository repo, INotificationFanoutQueue? fanoutQueue = null, IMemoryCache? cache = null)
    {
        _repo = repo;
        _fanoutQueue = fanoutQueue;
        _cache = cache ?? new MemoryCache(new MemoryCacheOptions());
    }

    /// <summary>
    /// Creates a notification and fans it out to the provided list of user IDs.
    /// Returns the new notification ID.
    /// </summary>
    public async Task<Guid> SendAsync(SendNotificationRequest request, Guid senderUserId, CancellationToken ct = default)
    {
        var notification = new Notification(request.Title, request.Body, request.Type, senderUserId);
        await _repo.AddAsync(notification, ct);
        await _repo.SaveChangesAsync(ct);

        await FanOutRecipientsAsync(notification.Id, request.RecipientUserIds, ct);
        BumpCacheVersion();

        return notification.Id;
    }

    /// <summary>
    /// Creates a system-generated notification and fans it out.
    /// Used by background jobs (no human sender ID).
    /// Returns the new notification ID.
    /// </summary>
    public async Task<Guid> SendSystemAsync(string title, string body, NotificationType type,
        IReadOnlyList<Guid> recipientUserIds, CancellationToken ct = default)
    {
        var notification = new Notification(title, body, type);
        await _repo.AddAsync(notification, ct);
        await _repo.SaveChangesAsync(ct);

        await FanOutRecipientsAsync(notification.Id, recipientUserIds, ct);
        BumpCacheVersion();

        return notification.Id;
    }

    /// <summary>
    /// Deactivates a notification, hiding it from all inboxes.
    /// Returns false when not found.
    /// </summary>
    public async Task<bool> DeactivateAsync(Guid notificationId, CancellationToken ct = default)
    {
        var notification = await _repo.GetByIdAsync(notificationId, ct);
        if (notification is null) return false;

        notification.Deactivate();
        _repo.Update(notification);
        await _repo.SaveChangesAsync(ct);
        BumpCacheVersion();
        return true;
    }

    /// <summary>Returns the user's inbox, optionally filtered to unread-only, with pagination.</summary>
    public async Task<IReadOnlyList<NotificationResponse>> GetInboxAsync(
        Guid userId, bool unreadOnly = false, int page = 0, int pageSize = 20, CancellationToken ct = default)
    {
        var cacheKey = $"notif:inbox:{CurrentCacheVersion()}:{userId}:{unreadOnly}:{page}:{pageSize}";
        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<NotificationResponse>? cached) && cached is not null)
            return cached;

        var records = await _repo.GetForUserAsync(userId, unreadOnly, page * pageSize, pageSize, asNoTracking: true, ct);
        var response = records.Select(ToResponse).ToList();
        _cache.Set(cacheKey, response, InboxCacheTtl);
        return response;
    }

    /// <summary>Returns the unread notification count for the notification bell badge.</summary>
    public async Task<NotificationBadgeResponse> GetBadgeAsync(Guid userId, CancellationToken ct = default)
    {
        var cacheKey = $"notif:badge:{CurrentCacheVersion()}:{userId}";
        if (_cache.TryGetValue(cacheKey, out NotificationBadgeResponse? cached) && cached is not null)
            return cached;

        var count = await _repo.GetUnreadCountAsync(userId, ct);
        var response = new NotificationBadgeResponse(count);
        _cache.Set(cacheKey, response, BadgeCacheTtl);
        return response;
    }

    /// <summary>
    /// Marks a specific notification as read for the user.
    /// Idempotent — returns true even if already read.
    /// Returns false when the delivery record is not found.
    /// </summary>
    public async Task<bool> MarkReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default)
    {
        var recipient = await _repo.GetRecipientAsync(notificationId, userId, ct);
        if (recipient is null) return false;

        recipient.MarkRead();
        _repo.UpdateRecipient(recipient);
        await _repo.SaveChangesAsync(ct);
        BumpCacheVersion();
        return true;
    }

    /// <summary>
    /// Marks all unread notifications as read for the user.
    /// Fetches a large page (500) to cover typical inbox sizes in one pass.
    /// </summary>
    public async Task MarkAllReadAsync(Guid userId, CancellationToken ct = default)
    {
        var unread = await _repo.GetForUserAsync(userId, unreadOnly: true, skip: 0, take: 500, asNoTracking: false, ct);
        foreach (var r in unread)
        {
            r.MarkRead();
            _repo.UpdateRecipient(r);
        }

        if (unread.Count > 0)
        {
            await _repo.SaveChangesAsync(ct);
            BumpCacheVersion();
        }
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    /// <summary>Maps a NotificationRecipient (with navigation) to a NotificationResponse DTO.</summary>
    private static NotificationResponse ToResponse(NotificationRecipient r) =>
        new(r.NotificationId,
            r.Notification.Title,
            r.Notification.Body,
            r.Notification.Type.ToString(),
            r.Notification.IsSystemGenerated,
            r.Notification.CreatedAt,
            r.IsRead,
            r.ReadAt);

    private async Task FanOutRecipientsAsync(Guid notificationId, IReadOnlyList<Guid> recipientUserIds, CancellationToken ct)
    {
        var distinctRecipients = recipientUserIds.Distinct().ToList();
        if (distinctRecipients.Count == 0)
            return;

        if (_fanoutQueue is not null && distinctRecipients.Count >= DeferredFanoutThreshold)
        {
            _fanoutQueue.Enqueue(new NotificationFanoutWorkItem(notificationId, distinctRecipients));
            return;
        }

        var recipients = distinctRecipients
            .Select(uid => new NotificationRecipient(notificationId, uid))
            .ToList();

        await _repo.AddRecipientsAsync(recipients, ct);
        await _repo.SaveChangesAsync(ct);
    }

    private static void BumpCacheVersion() => Interlocked.Increment(ref _cacheVersion);
    private static int CurrentCacheVersion() => Volatile.Read(ref _cacheVersion);
}
