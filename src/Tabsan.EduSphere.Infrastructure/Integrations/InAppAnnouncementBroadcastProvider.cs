using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Infrastructure.Integrations;

// Default announcement integration provider using enrollment fan-out + in-app notifications.

public sealed class InAppAnnouncementBroadcastProvider : IAnnouncementBroadcastProvider
{
    private readonly IEnrollmentRepository _enrollments;
    private readonly INotificationService _notifications;
    private readonly IOutboundIntegrationGateway _gateway;

    public InAppAnnouncementBroadcastProvider(
        IEnrollmentRepository enrollments,
        INotificationService notifications,
        IOutboundIntegrationGateway gateway)
    {
        _enrollments = enrollments;
        _notifications = notifications;
        _gateway = gateway;
    }

    public string ProviderKey => "inapp-fanout";

    public async Task<int> BroadcastAsync(Guid? offeringId, string title, string body, CancellationToken ct = default)
    {
        if (!offeringId.HasValue)
            return 0;

        var enrollments = await _enrollments.GetByOfferingAsync(offeringId.Value, ct);
        var recipientIds = enrollments
            .Where(e => e.Status == EnrollmentStatus.Active && e.StudentProfile is not null)
            .Select(e => e.StudentProfile!.UserId)
            .Distinct()
            .ToList();

        if (recipientIds.Count == 0)
            return 0;

        await _gateway.ExecuteAsync(
            channel: "push",
            operation: "announcement.broadcast",
            action: gatewayCt => _notifications.SendSystemAsync(title, body, NotificationType.Announcement, recipientIds, gatewayCt),
            ct);
        return recipientIds.Count;
    }
}
