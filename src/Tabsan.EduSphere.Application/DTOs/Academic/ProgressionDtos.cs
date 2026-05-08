using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.DTOs.Academic;

// Phase 25 — Academic Engine Unification — Stage 25.3

/// <summary>
/// Outcome of a progression evaluation for a single student.
/// </summary>
/// <param name="StudentProfileId">The student being evaluated.</param>
/// <param name="InstitutionType">The institution mode under which progression was evaluated.</param>
/// <param name="CanProgress">True when the student meets the promotion criteria.</param>
/// <param name="CurrentPeriodLabel">Human-readable label for the current academic period (e.g. "Semester 2", "Grade 5", "Year 1").</param>
/// <param name="NextPeriodLabel">Label for the period the student would progress to, or null if already at the end.</param>
/// <param name="AchievedScore">The computed score used in the progression decision (GPA or percentage, depending on mode).</param>
/// <param name="RequiredScore">The minimum score required to progress.</param>
/// <param name="Remarks">Optional human-readable notes (e.g. "CGPA 2.5 ≥ 2.0 — eligible for semester promotion").</param>
public sealed record ProgressionDecision(
    Guid StudentProfileId,
    InstitutionType InstitutionType,
    bool CanProgress,
    string CurrentPeriodLabel,
    string? NextPeriodLabel,
    decimal AchievedScore,
    decimal RequiredScore,
    string Remarks);

/// <summary>
/// Request to evaluate whether a student may progress to the next academic period.
/// </summary>
/// <param name="StudentProfileId">The student to evaluate.</param>
/// <param name="InstitutionType">The institution type governing this evaluation.</param>
public sealed record ProgressionEvaluationRequest(Guid StudentProfileId, InstitutionType InstitutionType);
