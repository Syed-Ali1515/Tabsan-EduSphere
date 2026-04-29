using Tabsan.EduSphere.Domain.Licensing;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Repository interface for reading and persisting the single LicenseState record
/// and tracking consumed VerificationKeys to prevent .tablic replay attacks.
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

    /// <summary>
    /// Returns true when the given VerificationKey hash has already been used to activate
    /// a license on this installation.  Used to block .tablic replay attacks.
    /// </summary>
    Task<bool> IsVerificationKeyConsumedAsync(string keyHash, CancellationToken ct = default);

    /// <summary>Persists a newly consumed VerificationKey hash record.</summary>
    Task AddConsumedKeyAsync(ConsumedVerificationKey key, CancellationToken ct = default);

    /// <summary>Commits pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
