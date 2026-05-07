namespace Tabsan.EduSphere.Application.DTOs.Academic;

// Final-Touches Phase 15 Stages 15.1 & 15.2 — enrollment rules DTOs: prerequisite + clash results

/// <summary>Rich result returned by <c>EnrollmentService.TryEnrollAsync</c>.</summary>
/// <param name="IsSuccess">True when the student was successfully enrolled.</param>
/// <param name="Enrollment">The enrollment record created on success; null on failure.</param>
/// <param name="RejectionReason">Human-readable summary reason when <c>IsSuccess</c> is false.</param>
/// <param name="UnmetPrerequisites">Course titles/codes that the student has not yet passed (Stage 15.1).</param>
/// <param name="ClashDetails">Schedule conflict descriptions — each entry names both clashing slots (Stage 15.2).</param>
public sealed record EnrollmentAttemptResult(
    bool IsSuccess,
    EnrollmentResponse? Enrollment = null,
    string? RejectionReason = null,
    IReadOnlyList<string>? UnmetPrerequisites = null,
    IReadOnlyList<string>? ClashDetails = null);

/// <summary>Response DTO for a single prerequisite link (used in the prerequisites management page).</summary>
public sealed record PrerequisiteDto(
    Guid CourseId,
    string CourseCode,
    string CourseTitle,
    Guid PrerequisiteCourseId,
    string PrerequisiteCourseCode,
    string PrerequisiteCourseTitle);

/// <summary>Request body for adding a prerequisite link.</summary>
public sealed record AddPrerequisiteRequest(Guid CourseId, Guid PrerequisiteCourseId);
