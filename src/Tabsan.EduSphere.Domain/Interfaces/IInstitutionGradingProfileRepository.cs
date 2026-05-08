using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Domain.Interfaces;

// Phase 25 — Academic Engine Unification — Stage 25.2

/// <summary>Repository interface for <see cref="InstitutionGradingProfile"/> persistence.</summary>
public interface IInstitutionGradingProfileRepository
{
    /// <summary>Returns all grading profiles.</summary>
    Task<IReadOnlyList<InstitutionGradingProfile>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns the active grading profile for the given institution type, or null.</summary>
    Task<InstitutionGradingProfile?> GetByTypeAsync(InstitutionType institutionType, CancellationToken ct = default);

    /// <summary>Returns a grading profile by its identifier, or null.</summary>
    Task<InstitutionGradingProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Queues a new profile for insertion.</summary>
    Task AddAsync(InstitutionGradingProfile profile, CancellationToken ct = default);

    /// <summary>Marks an existing profile as modified.</summary>
    void Update(InstitutionGradingProfile profile);

    /// <summary>Persists all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
