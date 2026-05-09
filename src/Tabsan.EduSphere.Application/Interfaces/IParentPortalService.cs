using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 26 — Stage 26.3

public interface IParentPortalService
{
    Task<IReadOnlyList<ParentLinkedStudentDto>> GetLinkedStudentsAsync(Guid parentUserId, CancellationToken ct = default);
}
