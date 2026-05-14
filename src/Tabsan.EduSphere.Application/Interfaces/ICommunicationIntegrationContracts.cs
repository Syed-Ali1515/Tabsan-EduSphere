namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 27.3 — Support and Communication Integration contracts.

public interface ISupportTicketingProvider
{
    string ProviderKey { get; }

    Task NotifyTicketReplyAsync(Guid submitterUserId, string ticketSubject, string responderDisplayName, CancellationToken ct = default);
    Task NotifyTicketAssignedAsync(Guid assigneeUserId, string ticketSubject, CancellationToken ct = default);
    Task NotifyTicketResolvedAsync(Guid submitterUserId, string ticketSubject, CancellationToken ct = default);
}

public interface IAnnouncementBroadcastProvider
{
    string ProviderKey { get; }

    Task<int> BroadcastAsync(Guid? offeringId, string title, string body, CancellationToken ct = default);
}

public interface IEmailDeliveryProvider
{
    string ProviderKey { get; }

    Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
    Task SendTemplateAsync(string to, string subject, string templateName, IDictionary<string, string> tokens, CancellationToken ct = default);
}

public interface ISmsDeliveryProvider
{
    string ProviderKey { get; }

    Task SendAsync(string to, string body, CancellationToken ct = default);
    Task SendTemplateAsync(string to, string templateName, IDictionary<string, string> tokens, CancellationToken ct = default);
}

public sealed record CommunicationIntegrationProfile(
    string TicketingProvider,
    string AnnouncementProvider,
    string EmailProvider);

public interface ICommunicationIntegrationService
{
    Task<CommunicationIntegrationProfile> GetProfileAsync(CancellationToken ct = default);
}
