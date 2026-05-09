using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Application.Services;

// Phase 27.3 — service to expose active communication provider contracts.

public sealed class CommunicationIntegrationService : ICommunicationIntegrationService
{
    private readonly ISupportTicketingProvider _ticketing;
    private readonly IAnnouncementBroadcastProvider _announcements;
    private readonly IEmailDeliveryProvider _email;

    public CommunicationIntegrationService(
        ISupportTicketingProvider ticketing,
        IAnnouncementBroadcastProvider announcements,
        IEmailDeliveryProvider email)
    {
        _ticketing = ticketing;
        _announcements = announcements;
        _email = email;
    }

    public Task<CommunicationIntegrationProfile> GetProfileAsync(CancellationToken ct = default)
        => Task.FromResult(new CommunicationIntegrationProfile(
            TicketingProvider: _ticketing.ProviderKey,
            AnnouncementProvider: _announcements.ProviderKey,
            EmailProvider: _email.ProviderKey));
}
