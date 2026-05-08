namespace Tabsan.EduSphere.Application.DTOs.Academic;

// Final-Touches Phase 19 Stage 19.4 — DTOs for per-course grading configuration

/// <summary>Response DTO for a course grading configuration.</summary>
public sealed record CourseGradingConfigDto(
    Guid Id,
    Guid CourseId,
    decimal PassThreshold,
    string GradingType,
    string? GradeRangesJson);

/// <summary>Request DTO to save (create or update) a course grading configuration.</summary>
public sealed record SaveCourseGradingConfigRequest(
    decimal PassThreshold,
    string GradingType,
    string? GradeRangesJson);

// Final-Touches Phase 19 Stage 19.1 — DTO returned after auto-creating semesters
/// <summary>Result of the AutoCreateSemesters operation.</summary>
public sealed record AutoCreateSemestersResult(int SemestersCreated, IReadOnlyList<Guid> SemesterIds);
