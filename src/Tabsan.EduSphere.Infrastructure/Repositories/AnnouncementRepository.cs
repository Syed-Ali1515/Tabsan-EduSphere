using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Lms;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

// Final-Touches Phase 20 Stage 20.4 — announcement repository

/// <summary>EF Core implementation of IAnnouncementRepository.</summary>
public sealed class AnnouncementRepository : IAnnouncementRepository
{
    private readonly ApplicationDbContext _db;
    public AnnouncementRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<CourseAnnouncement>> GetByOfferingAsync(
        Guid offeringId, CancellationToken ct = default)
        => await _db.CourseAnnouncements
                    .Where(a => a.OfferingId == offeringId)
                    .OrderByDescending(a => a.PostedAt)
                    .ToListAsync(ct);

    public async Task<CourseAnnouncement?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await _db.CourseAnnouncements.FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task AddAsync(CourseAnnouncement announcement, CancellationToken ct = default)
        => await _db.CourseAnnouncements.AddAsync(announcement, ct);

    public void Update(CourseAnnouncement announcement)
        => _db.CourseAnnouncements.Update(announcement);

    public void Delete(CourseAnnouncement announcement)
        => _db.CourseAnnouncements.Remove(announcement);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
