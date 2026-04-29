using Tabsan.EduSphere.Domain.Licensing;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Repository interface for reading and persisting the single LicenseState record.
/// </summary>
public interface ILicenseRepository
{
    /// <summary>
    /// Returns the current license state row, or null if no license has been activated yet.
    /// There is always at most one row in the license_state table.
    /// </summary>
    Task<LicenseState?> GetCurrentAsync(CancellationToken ct = default);

    /// <summary>Persists a newly created LicenseState (first activation).</summary>
    Task AddAsync(LicenseState state, CancellationToken ct = default);

    /// <summary>Persists changes to the existing LicenseState row (renewal, status refresh).</summary>
    void Update(LicenseState state);

    /// <summary>Commits pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
