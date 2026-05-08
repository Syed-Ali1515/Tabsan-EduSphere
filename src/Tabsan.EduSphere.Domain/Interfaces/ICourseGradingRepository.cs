using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

// Final-Touches Phase 19 Stage 19.4 — repository interface for per-course grading configuration

/// <summary>Repository interface for CourseGradingConfig operations.</summary>
public interface ICourseGradingRepository
{
    /// <summary>Returns the grading configuration for the given course, or null.</summary>
    Task<CourseGradingConfig?> GetByCourseIdAsync(Guid courseId, CancellationToken ct = default);

    /// <summary>Queues a new config for insertion.</summary>
    Task AddAsync(CourseGradingConfig config, CancellationToken ct = default);

    /// <summary>Marks the config as modified.</summary>
    void Update(CourseGradingConfig config);

    /// <summary>Commits changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
