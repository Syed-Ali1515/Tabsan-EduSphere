using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.StudyPlanner;

namespace Tabsan.EduSphere.Application.DTOs.StudyPlanner;

// Final-Touches Phase 21 Stage 21.1 — Study Planner DTOs

// ──── Requests ───────────────────────────────────────────────────────────────

/// <summary>Request to create a new study plan for a student.</summary>
public record CreateStudyPlanRequest(
    Guid   StudentProfileId,
    string PlannedSemesterName,
    string? Notes);

/// <summary>Request to add a course to an existing study plan.</summary>
public record AddPlanCourseRequest(
    Guid PlanId,
    Guid CourseId);

/// <summary>Request for a faculty advisor to endorse or reject a plan.</summary>
public record AdvisePlanRequest(
    Guid    PlanId,
    bool    IsEndorsed,
    string? AdvisorNotes);

/// <summary>Request to update the credit-load limit on an academic programme (SuperAdmin).</summary>
public record SetMaxCreditLoadRequest(
    Guid ProgramId,
    int  MaxCreditLoadPerSemester);

// ──── Response DTOs ───────────────────────────────────────────────────────────

/// <summary>A single course included in a study plan.</summary>
public record StudyPlanCourseDto(
    Guid        CourseId,
    string      CourseCode,
    string      CourseTitle,
    int         CreditHours,
    CourseType  CourseType);

/// <summary>Full representation of a student's study plan.</summary>
public record StudyPlanDto(
    Guid                          Id,
    Guid                          StudentProfileId,
    string                        PlannedSemesterName,
    string?                       Notes,
    StudyPlanStatus               AdvisorStatus,
    string?                       AdvisorNotes,
    Guid?                         ReviewedByUserId,
    int                           TotalCreditHours,
    IReadOnlyList<StudyPlanCourseDto> Courses,
    DateTime                      CreatedAt,
    DateTime?                     UpdatedAt);

// ──── Recommendation DTOs (Stage 21.2) ───────────────────────────────────────

/// <summary>A course recommended for inclusion in a student's next study plan.</summary>
public record RecommendedCourseDto(
    Guid       CourseId,
    string     CourseCode,
    string     CourseTitle,
    int        CreditHours,
    CourseType CourseType,
    string     Reason);

/// <summary>Recommendation response containing recommended courses for a planned semester.</summary>
public record StudyPlanRecommendationDto(
    Guid                              StudentProfileId,
    string                            PlannedSemesterName,
    int                               MaxCreditLoad,
    int                               RecommendedTotalCredits,
    IReadOnlyList<RecommendedCourseDto> Recommendations);
