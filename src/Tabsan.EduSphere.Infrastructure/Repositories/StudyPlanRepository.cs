using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.StudyPlanner;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

// Final-Touches Phase 21 Stage 21.1 — Study Planner repository implementation

/// <summary>EF Core implementation of <see cref="IStudyPlanRepository"/>.</summary>
public sealed class StudyPlanRepository : IStudyPlanRepository
{
    private readonly ApplicationDbContext _db;

    public StudyPlanRepository(ApplicationDbContext db) => _db = db;

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudyPlan>> GetByStudentAsync(Guid studentProfileId, CancellationToken ct = default)
        => await _db.StudyPlans
                    .Include(p => p.Courses)
                        .ThenInclude(c => c.Course)
                    .Where(p => p.StudentProfileId == studentProfileId)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<IReadOnlyList<StudyPlan>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default)
        => await _db.StudyPlans
                    .Include(p => p.Courses)
                        .ThenInclude(c => c.Course)
                    .Where(p => _db.StudentProfiles
                                   .Any(s => s.Id == p.StudentProfileId && s.DepartmentId == departmentId))
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync(ct);

    /// <inheritdoc />
    public async Task<StudyPlan?> GetByIdAsync(Guid planId, CancellationToken ct = default)
        => await _db.StudyPlans
                    .Include(p => p.Courses)
                        .ThenInclude(c => c.Course)
                    .FirstOrDefaultAsync(p => p.Id == planId, ct);

    /// <inheritdoc />
    public async Task AddAsync(StudyPlan plan, CancellationToken ct = default)
        => await _db.StudyPlans.AddAsync(plan, ct);

    /// <inheritdoc />
    public void Update(StudyPlan plan) => _db.StudyPlans.Update(plan);

    /// <inheritdoc />
    public async Task<int> GetPlannedCreditHoursAsync(Guid studentProfileId, string semesterName, CancellationToken ct = default)
    {
        // Sum credit hours across all plans for this student + semester (excluding soft-deleted plans via query filter)
        return await _db.StudyPlans
                        .Include(p => p.Courses)
                            .ThenInclude(c => c.Course)
                        .Where(p => p.StudentProfileId == studentProfileId
                                 && p.PlannedSemesterName == semesterName)
                        .SelectMany(p => p.Courses)
                        .SumAsync(c => c.Course != null ? c.Course.CreditHours : 0, ct);
    }

    /// <inheritdoc />
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
