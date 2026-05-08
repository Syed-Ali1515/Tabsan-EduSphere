using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 25 — Academic Engine Unification — Stage 25.3

/// <summary>
/// Evaluates whether a student is eligible to progress to the next academic period
/// (semester / grade / year) based on their current results and the active grading
/// profile for their institution type.
/// </summary>
public interface IProgressionService
{
    /// <summary>
    /// Evaluates progression eligibility for the specified student under the given
    /// institution type.  The service loads the student's published results and the
    /// active grading profile to produce a <see cref="ProgressionDecision"/>.
    /// </summary>
    Task<ProgressionDecision> EvaluateAsync(ProgressionEvaluationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Promotes the student to the next academic period if they are eligible.
    /// Calls <see cref="EvaluateAsync"/> internally; throws <see cref="InvalidOperationException"/>
    /// when the student does not meet the criteria.
    /// Returns the updated progression decision reflecting the new period.
    /// </summary>
    Task<ProgressionDecision> PromoteAsync(ProgressionEvaluationRequest request, CancellationToken ct = default);
}
