using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

// Final-Touches Phase 19 Stage 19.4 — service interface for per-course grading config

/// <summary>Application service for per-course grading configuration.</summary>
public interface ICourseGradingService
{
    /// <summary>Returns the grading config for the given course, or null if not set.</summary>
    Task<CourseGradingConfigDto?> GetConfigAsync(Guid courseId, CancellationToken ct = default);

    /// <summary>Creates or updates the grading config for the given course.</summary>
    Task<CourseGradingConfigDto> UpsertConfigAsync(Guid courseId, SaveCourseGradingConfigRequest request, CancellationToken ct = default);
}
