using Tabsan.EduSphere.Application.DTOs.Notifications;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Contract for composing, dispatching, and managing notifications.
/// Faculty/Admins send notifications; students receive and read them.
/// The system dispatches notifications automatically via background jobs.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a specific list of users.
    /// Creates one NotificationRecipient row per recipient (fan-out).
    /// Returns the created notification ID.
    /// </summary>
    Task<Guid> SendAsync(SendNotificationRequest request, Guid senderUserId, CancellationToken ct = default);

    /// <summary>
    /// Sends a system-generated notification (no human sender).
    /// Used by background jobs such as the attendance alert job.
    /// Returns the created notification ID.
    /// </summary>
    Task<Guid> SendSystemAsync(string title, string body, NotificationType type,
        IReadOnlyList<Guid> recipientUserIds, CancellationToken ct = default);

    /// <summary>
    /// Deactivates a notification, hiding it from all user inboxes.
    /// Returns false when not found.
    /// </summary>
    Task<bool> DeactivateAsync(Guid notificationId, CancellationToken ct = default);

    /// <summary>
    /// Returns paged inbox notifications for a user.
    /// Supports filtering to unread-only via <paramref name="unreadOnly"/>.
    /// </summary>
    Task<IReadOnlyList<NotificationResponse>> GetInboxAsync(
        Guid userId, bool unreadOnly = false, int page = 0, int pageSize = 20, CancellationToken ct = default);

    /// <summary>Returns the unread notification count for the user's badge indicator.</summary>
    Task<NotificationBadgeResponse> GetBadgeAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Marks a notification as read for the given user.
    /// Idempotent — safe to call multiple times.
    /// Returns false when the delivery record is not found.
    /// </summary>
    Task<bool> MarkReadAsync(Guid notificationId, Guid userId, CancellationToken ct = default);

    /// <summary>Marks all unread notifications as read for the user.</summary>
    Task MarkAllReadAsync(Guid userId, CancellationToken ct = default);
}
