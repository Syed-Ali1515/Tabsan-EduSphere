using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Infrastructure.Integrations;

// Default support-ticketing integration provider using in-app notifications.

public sealed class InAppSupportTicketingProvider : ISupportTicketingProvider
{
    private readonly INotificationService _notifications;
    private readonly IOutboundIntegrationGateway _gateway;

    public InAppSupportTicketingProvider(
        INotificationService notifications,
        IOutboundIntegrationGateway gateway)
    {
        _notifications = notifications;
        _gateway = gateway;
    }

    public string ProviderKey => "inapp-notification";

    public Task NotifyTicketReplyAsync(Guid submitterUserId, string ticketSubject, string responderDisplayName, CancellationToken ct = default)
        => _gateway.ExecuteAsync(
            channel: "push",
            operation: "support.reply",
            action: gatewayCt => _notifications.SendSystemAsync(
                $"New reply on ticket: {ticketSubject}",
                $"{responderDisplayName} replied to your support ticket.",
                NotificationType.System,
                [submitterUserId],
                gatewayCt),
            ct);

    public Task NotifyTicketAssignedAsync(Guid assigneeUserId, string ticketSubject, CancellationToken ct = default)
        => _gateway.ExecuteAsync(
            channel: "push",
            operation: "support.assigned",
            action: gatewayCt => _notifications.SendSystemAsync(
                "Support ticket assigned to you",
                $"Ticket \"{ticketSubject}\" has been assigned to you.",
                NotificationType.System,
                [assigneeUserId],
                gatewayCt),
            ct);

    public Task NotifyTicketResolvedAsync(Guid submitterUserId, string ticketSubject, CancellationToken ct = default)
        => _gateway.ExecuteAsync(
            channel: "push",
            operation: "support.resolved",
            action: gatewayCt => _notifications.SendSystemAsync(
                "Your support ticket has been resolved",
                $"Ticket \"{ticketSubject}\" has been marked as resolved.",
                NotificationType.System,
                [submitterUserId],
                gatewayCt),
            ct);
}
