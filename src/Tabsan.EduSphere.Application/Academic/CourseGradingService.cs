using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Academic;

// Final-Touches Phase 19 Stage 19.4 — per-course grading configuration service

/// <summary>Application service for per-course grading configuration.</summary>
public class CourseGradingService : ICourseGradingService
{
    private readonly ICourseGradingRepository _repo;
    private readonly ICourseRepository _courseRepo;

    public CourseGradingService(ICourseGradingRepository repo, ICourseRepository courseRepo)
    {
        _repo = repo;
        _courseRepo = courseRepo;
    }

    // Final-Touches Phase 19 Stage 19.4 — get per-course grading config
    public async Task<CourseGradingConfigDto?> GetConfigAsync(Guid courseId, CancellationToken ct = default)
    {
        var config = await _repo.GetByCourseIdAsync(courseId, ct);
        return config is null ? null : Map(config);
    }

    // Final-Touches Phase 19 Stage 19.4 — upsert per-course grading config
    public async Task<CourseGradingConfigDto> UpsertConfigAsync(Guid courseId, SaveCourseGradingConfigRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.GradingType))
            throw new ArgumentException("GradingType is required.");
        if (request.PassThreshold < 0 || request.PassThreshold > 100)
            throw new ArgumentOutOfRangeException(nameof(request.PassThreshold), "PassThreshold must be between 0 and 100.");

        // Ensure course exists
        _ = await _courseRepo.GetByIdAsync(courseId, ct)
            ?? throw new InvalidOperationException($"Course '{courseId}' not found.");

        var existing = await _repo.GetByCourseIdAsync(courseId, ct);

        if (existing is null)
        {
            var config = new CourseGradingConfig(courseId, request.PassThreshold, request.GradingType, request.GradeRangesJson);
            await _repo.AddAsync(config, ct);
            await _repo.SaveChangesAsync(ct);
            return Map(config);
        }
        else
        {
            existing.Update(request.PassThreshold, request.GradingType, request.GradeRangesJson);
            _repo.Update(existing);
            await _repo.SaveChangesAsync(ct);
            return Map(existing);
        }
    }

    private static CourseGradingConfigDto Map(CourseGradingConfig c) =>
        new(c.Id, c.CourseId, c.PassThreshold, c.GradingType, c.GradeRangesJson);
}
