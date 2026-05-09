using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Infrastructure.Integrations;

// Default support-ticketing integration provider using in-app notifications.

public sealed class InAppSupportTicketingProvider : ISupportTicketingProvider
{
    private readonly INotificationService _notifications;

    public InAppSupportTicketingProvider(INotificationService notifications)
    {
        _notifications = notifications;
    }

    public string ProviderKey => "inapp-notification";

    public Task NotifyTicketReplyAsync(Guid submitterUserId, string ticketSubject, string responderDisplayName, CancellationToken ct = default)
        => _notifications.SendSystemAsync(
            $"New reply on ticket: {ticketSubject}",
            $"{responderDisplayName} replied to your support ticket.",
            NotificationType.System,
            [submitterUserId],
            ct);

    public Task NotifyTicketAssignedAsync(Guid assigneeUserId, string ticketSubject, CancellationToken ct = default)
        => _notifications.SendSystemAsync(
            "Support ticket assigned to you",
            $"Ticket \"{ticketSubject}\" has been assigned to you.",
            NotificationType.System,
            [assigneeUserId],
            ct);

    public Task NotifyTicketResolvedAsync(Guid submitterUserId, string ticketSubject, CancellationToken ct = default)
        => _notifications.SendSystemAsync(
            "Your support ticket has been resolved",
            $"Ticket \"{ticketSubject}\" has been marked as resolved.",
            NotificationType.System,
            [submitterUserId],
            ct);
}
