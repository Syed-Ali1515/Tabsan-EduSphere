// Final-Touches Phase 16 Stage 16.2 — rubric service interface

using Tabsan.EduSphere.Application.DTOs.Assignments;

namespace Tabsan.EduSphere.Application.Interfaces;

public interface IRubricService
{
    // Final-Touches Phase 16 Stage 16.2 — fetch rubric for an assignment
    Task<RubricResponse?> GetByAssignmentAsync(Guid assignmentId, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.2 — create a new rubric with criteria and levels
    Task<Guid> CreateAsync(CreateRubricRequest request, Guid createdByUserId, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.2 — update rubric title
    Task UpdateAsync(Guid rubricId, UpdateRubricRequest request, Guid updatedByUserId, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.2 — soft-delete (deactivate) a rubric
    Task DeleteAsync(Guid rubricId, Guid updatedByUserId, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.2 — grade a submission using rubric level selections
    Task<RubricGradeResponse> GradeSubmissionAsync(RubricGradeRequest request, Guid gradedByUserId, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.2 — retrieve current rubric grade for a submission
    Task<RubricGradeResponse?> GetSubmissionGradeAsync(Guid rubricId, Guid submissionId, CancellationToken ct = default);
}
