using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>EF Core implementation of IAcademicDeadlineRepository.</summary>
public class AcademicDeadlineRepository : IAcademicDeadlineRepository
{
    private readonly ApplicationDbContext _db;
    public AcademicDeadlineRepository(ApplicationDbContext db) => _db = db;

    public async Task<IReadOnlyList<AcademicDeadline>> GetBySemesterAsync(Guid semesterId, CancellationToken ct = default)
        => await _db.AcademicDeadlines
                    .Include(d => d.Semester)
                    .Where(d => d.SemesterId == semesterId && d.IsActive)
                    .OrderBy(d => d.DeadlineDate)
                    .ToListAsync(ct);

    public async Task<IReadOnlyList<AcademicDeadline>> GetAllActiveAsync(CancellationToken ct = default)
        => await _db.AcademicDeadlines
                    .Include(d => d.Semester)
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DeadlineDate)
                    .ToListAsync(ct);

    public async Task<IReadOnlyList<AcademicDeadline>> GetPendingRemindersAsync(DateTime utcNow, CancellationToken ct = default)
        => await _db.AcademicDeadlines
                    .Where(d => d.IsActive
                             && d.LastReminderSentAt == null
                             && d.ReminderDaysBefore > 0
                             && d.DeadlineDate.AddDays(-d.ReminderDaysBefore) <= utcNow)
                    .ToListAsync(ct);

    public Task<AcademicDeadline?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.AcademicDeadlines.Include(d => d.Semester).FirstOrDefaultAsync(d => d.Id == id, ct);

    public async Task AddAsync(AcademicDeadline deadline, CancellationToken ct = default)
        => await _db.AcademicDeadlines.AddAsync(deadline, ct);

    public void Update(AcademicDeadline deadline) => _db.AcademicDeadlines.Update(deadline);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
