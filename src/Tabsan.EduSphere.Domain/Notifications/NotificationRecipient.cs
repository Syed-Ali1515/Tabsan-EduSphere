using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Notifications;

/// <summary>
/// Tracks the delivery and read-state of a <see cref="Notification"/> for a single recipient user.
/// One row exists per (Notification, User) pair.
/// Recipients are created when the notification is dispatched; they are never soft-deleted.
/// </summary>
public class NotificationRecipient : BaseEntity
{
    /// <summary>FK to the notification being delivered.</summary>
    public Guid NotificationId { get; private set; }

    /// <summary>Navigation to the parent notification.</summary>
    public Notification Notification { get; private set; } = default!;

    /// <summary>FK to the user receiving this notification.</summary>
    public Guid RecipientUserId { get; private set; }

    /// <summary>True once the recipient has opened or acknowledged the notification.</summary>
    public bool IsRead { get; private set; }

    /// <summary>UTC timestamp at which the recipient marked the notification as read. Null if unread.</summary>
    public DateTime? ReadAt { get; private set; }

    // ── EF constructor ────────────────────────────────────────────────────────
    private NotificationRecipient() { }

    /// <summary>Creates an unread delivery record for the given user.</summary>
    public NotificationRecipient(Guid notificationId, Guid recipientUserId)
    {
        NotificationId  = notificationId;
        RecipientUserId = recipientUserId;
        IsRead          = false;
        Touch();
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Marks the notification as read.
    /// Idempotent — calling again on an already-read record has no effect.
    /// </summary>
    public void MarkRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
        Touch();
    }
}
