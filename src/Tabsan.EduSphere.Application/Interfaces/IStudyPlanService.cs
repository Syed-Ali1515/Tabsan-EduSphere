using Tabsan.EduSphere.Application.DTOs.StudyPlanner;

namespace Tabsan.EduSphere.Application.Interfaces;

// Final-Touches Phase 21 Stage 21.1/21.2 — Study Planner service contract

/// <summary>Service for creating and managing student semester study plans.</summary>
public interface IStudyPlanService
{
    // ── Stage 21.1: Plan CRUD ─────────────────────────────────────────────────

    /// <summary>Returns all study plans for the given student profile.</summary>
    Task<List<StudyPlanDto>> GetPlansAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Returns all plans for all students in a department (faculty advisor view).</summary>
    Task<List<StudyPlanDto>> GetPlansByDepartmentAsync(Guid departmentId, CancellationToken ct = default);

    /// <summary>Returns a single study plan by its ID, or null.</summary>
    Task<StudyPlanDto?> GetPlanAsync(Guid planId, CancellationToken ct = default);

    /// <summary>Creates a new study plan. Student cannot have two plans for the same semester.</summary>
    Task<StudyPlanDto> CreatePlanAsync(CreateStudyPlanRequest request, CancellationToken ct = default);

    /// <summary>Adds a course to a study plan. Validates prerequisites and credit-load limit.</summary>
    Task<StudyPlanDto> AddCourseAsync(AddPlanCourseRequest request, CancellationToken ct = default);

    /// <summary>Removes a course from a study plan and resets advisor status to Pending.</summary>
    Task RemoveCourseAsync(Guid planId, Guid courseId, CancellationToken ct = default);

    /// <summary>Soft-deletes a study plan.</summary>
    Task DeletePlanAsync(Guid planId, CancellationToken ct = default);

    // ── Stage 21.1: Advisor workflow ──────────────────────────────────────────

    /// <summary>Faculty advisor endorses or rejects a student plan.</summary>
    Task AdvisePlanAsync(AdvisePlanRequest request, Guid advisorUserId, CancellationToken ct = default);

    // ── Stage 21.2: Recommendations ──────────────────────────────────────────

    /// <summary>
    /// Returns an auto-generated course recommendation list for the student's next semester,
    /// based on degree gaps, prerequisite availability, and credit-load limits.
    /// </summary>
    Task<StudyPlanRecommendationDto> GetRecommendationsAsync(
        Guid   studentProfileId,
        string plannedSemesterName,
        CancellationToken ct = default);
}
