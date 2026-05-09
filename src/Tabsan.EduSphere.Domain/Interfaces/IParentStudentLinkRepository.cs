using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

// Phase 26 — Stage 26.3

/// <summary>Repository abstraction for parent-student mapping.</summary>
public interface IParentStudentLinkRepository
{
    Task<IReadOnlyList<ParentStudentLink>> GetByParentUserIdAsync(Guid parentUserId, CancellationToken ct = default);

    Task<ParentStudentLink?> GetByParentAndStudentAsync(Guid parentUserId, Guid studentProfileId, CancellationToken ct = default);

    Task AddAsync(ParentStudentLink link, CancellationToken ct = default);

    void Update(ParentStudentLink link);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
