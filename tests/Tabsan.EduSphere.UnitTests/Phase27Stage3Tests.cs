using FluentAssertions;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Application.Services;
using Xunit;

namespace Tabsan.EduSphere.UnitTests;

// Phase 27.3 — Support and Communication Integration unit tests

public class CommunicationIntegrationServiceTests
{
    [Fact]
    public async Task GetProfileAsync_ReturnsProviderKeys()
    {
        var sut = new CommunicationIntegrationService(
            new StubTicketingProvider(),
            new StubAnnouncementProvider(),
            new StubEmailProvider());

        var profile = await sut.GetProfileAsync();

        profile.TicketingProvider.Should().Be("ticketing-stub");
        profile.AnnouncementProvider.Should().Be("announcement-stub");
        profile.EmailProvider.Should().Be("email-stub");
    }
}

file sealed class StubTicketingProvider : ISupportTicketingProvider
{
    public string ProviderKey => "ticketing-stub";

    public Task NotifyTicketReplyAsync(Guid submitterUserId, string ticketSubject, string responderDisplayName, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotifyTicketAssignedAsync(Guid assigneeUserId, string ticketSubject, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotifyTicketResolvedAsync(Guid submitterUserId, string ticketSubject, CancellationToken ct = default)
        => Task.CompletedTask;
}

file sealed class StubAnnouncementProvider : IAnnouncementBroadcastProvider
{
    public string ProviderKey => "announcement-stub";

    public Task<int> BroadcastAsync(Guid? offeringId, string title, string body, CancellationToken ct = default)
        => Task.FromResult(0);
}

file sealed class StubEmailProvider : IEmailDeliveryProvider
{
    public string ProviderKey => "email-stub";

    public Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task SendTemplateAsync(string to, string subject, string templateName, IDictionary<string, string> tokens, CancellationToken ct = default)
        => Task.CompletedTask;
}
