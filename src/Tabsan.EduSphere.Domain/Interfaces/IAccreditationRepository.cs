// Final-Touches Phase 22 Stage 22.2 — IAccreditationRepository interface
using Tabsan.EduSphere.Domain.Settings;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository contract for accreditation template CRUD.</summary>
public interface IAccreditationRepository
{
    Task<IList<AccreditationTemplate>> GetAllAsync(CancellationToken ct = default);
    Task<AccreditationTemplate?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(AccreditationTemplate template, CancellationToken ct = default);
    void Remove(AccreditationTemplate template);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
