using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Application.DTOs.Notifications;

// ── Notification DTOs ─────────────────────────────────────────────────────────

/// <summary>Request body for composing and dispatching a notification to specific users.</summary>
public sealed record SendNotificationRequest(
    string Title,
    string Body,
    NotificationType Type,
    IReadOnlyList<Guid> RecipientUserIds);

/// <summary>Request body for broadcasting a notification to all users in a department.</summary>
public sealed record BroadcastNotificationRequest(
    string Title,
    string Body,
    NotificationType Type,
    Guid DepartmentId);

/// <summary>Read-model for a notification in a user's inbox.</summary>
public sealed record NotificationResponse(
    Guid NotificationId,
    string Title,
    string Body,
    string Type,
    bool IsSystemGenerated,
    DateTime SentAt,
    bool IsRead,
    DateTime? ReadAt);

/// <summary>Summary badge counts for the notification bell.</summary>
public sealed record NotificationBadgeResponse(int UnreadCount);
