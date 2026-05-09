using Tabsan.EduSphere.Application.DTOs.Lms;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Lms;

namespace Tabsan.EduSphere.Application.Lms;

// Final-Touches Phase 20 Stage 20.4 — announcement service implementation

/// <summary>
/// Application service for Phase 20 — course announcements.
/// Dispatches in-app notifications (NotificationType.Announcement = 6) to enrolled students.
/// </summary>
public sealed class AnnouncementService : IAnnouncementService
{
    private readonly IAnnouncementRepository _repo;
    private readonly IUserRepository         _users;
    private readonly IAnnouncementBroadcastProvider _broadcastProvider;

    public AnnouncementService(
        IAnnouncementRepository repo,
        IUserRepository         users,
        IAnnouncementBroadcastProvider broadcastProvider)
    {
        _repo          = repo;
        _users         = users;
        _broadcastProvider = broadcastProvider;
    }

    public async Task<List<CourseAnnouncementDto>> GetByOfferingAsync(
        Guid offeringId, CancellationToken ct = default)
    {
        var items  = await _repo.GetByOfferingAsync(offeringId, ct);
        var result = new List<CourseAnnouncementDto>(items.Count);
        foreach (var a in items)
        {
            var author = await _users.GetByIdAsync(a.AuthorId, ct);
            result.Add(MapAnnouncement(a, author?.Username ?? "Unknown"));
        }
        return result;
    }

    public async Task<CourseAnnouncementDto> CreateAsync(
        CreateAnnouncementRequest request, CancellationToken ct = default)
    {
        var announcement = new CourseAnnouncement(
            request.OfferingId, request.AuthorId, request.Title, request.Body);

        await _repo.AddAsync(announcement, ct);
        await _repo.SaveChangesAsync(ct);

        await _broadcastProvider.BroadcastAsync(request.OfferingId, request.Title, request.Body, ct);

        var author = await _users.GetByIdAsync(announcement.AuthorId, ct);
        return MapAnnouncement(announcement, author?.Username ?? "Unknown");
    }

    public async Task DeleteAsync(Guid announcementId, CancellationToken ct = default)
    {
        var a = await _repo.GetByIdAsync(announcementId, ct)
            ?? throw new InvalidOperationException($"Announcement {announcementId} not found.");
        a.SoftDelete();
        _repo.Update(a);
        await _repo.SaveChangesAsync(ct);
    }

    // ── Mapper ─────────────────────────────────────────────────────────────────

    private static CourseAnnouncementDto MapAnnouncement(CourseAnnouncement a, string authorName) => new()
    {
        Id         = a.Id,
        OfferingId = a.OfferingId,
        AuthorId   = a.AuthorId,
        AuthorName = authorName,
        Title      = a.Title,
        Body       = a.Body,
        PostedAt   = a.PostedAt
    };
}
