using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Persistence contract for notifications and their per-user delivery records.
/// </summary>
public interface INotificationRepository
{
    // ── Notifications ─────────────────────────────────────────────────────────

    /// <summary>Returns the notification by ID, or null.</summary>
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Queues a notification for insertion.</summary>
    Task AddAsync(Notification notification, CancellationToken ct = default);

    /// <summary>Marks a notification as modified (e.g. deactivated).</summary>
    void Update(Notification notification);

    // ── Recipients ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns paged notifications for a user, newest first.
    /// Includes both read and unread unless <paramref name="unreadOnly"/> is true.
    /// </summary>
    Task<IReadOnlyList<NotificationRecipient>> GetForUserAsync(
        Guid userId, bool unreadOnly = false, int skip = 0, int take = 20, CancellationToken ct = default);

    /// <summary>Returns the count of unread notifications for a user.</summary>
    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the delivery record for a specific user+notification pair, or null.</summary>
    Task<NotificationRecipient?> GetRecipientAsync(Guid notificationId, Guid userId, CancellationToken ct = default);

    /// <summary>Queues multiple recipient rows for bulk insertion (fan-out).</summary>
    Task AddRecipientsAsync(IEnumerable<NotificationRecipient> recipients, CancellationToken ct = default);

    /// <summary>Marks a recipient row as modified (read state updated).</summary>
    void UpdateRecipient(NotificationRecipient recipient);

    /// <summary>Commits pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
