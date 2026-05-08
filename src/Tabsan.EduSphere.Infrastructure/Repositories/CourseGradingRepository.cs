using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

// Final-Touches Phase 19 Stage 19.4 — EF Core implementation of ICourseGradingRepository

/// <summary>EF Core implementation of ICourseGradingRepository.</summary>
public class CourseGradingRepository : ICourseGradingRepository
{
    private readonly ApplicationDbContext _db;
    public CourseGradingRepository(ApplicationDbContext db) => _db = db;

    public Task<CourseGradingConfig?> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default)
        => _db.CourseGradingConfigs.FirstOrDefaultAsync(c => c.CourseId == courseId, ct);

    public Task AddAsync(CourseGradingConfig config, CancellationToken ct = default)
    {
        _db.CourseGradingConfigs.Add(config);
        return Task.CompletedTask;
    }

    public void Update(CourseGradingConfig config) => _db.CourseGradingConfigs.Update(config);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
