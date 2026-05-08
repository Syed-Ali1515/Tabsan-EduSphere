using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

// Final-Touches Phase 19 Stage 19.1 — service interface for course operations

/// <summary>Application service for advanced course operations (Phase 19).</summary>
public interface ICourseService
{
    /// <summary>
    /// Auto-creates TotalSemesters Semester rows linked to the course code.
    /// Skips creation of any semester whose name already exists (idempotent).
    /// Returns the count of newly created semesters.
    /// </summary>
    Task<AutoCreateSemestersResult> AutoCreateSemestersAsync(Guid courseId, CancellationToken ct = default);
}
