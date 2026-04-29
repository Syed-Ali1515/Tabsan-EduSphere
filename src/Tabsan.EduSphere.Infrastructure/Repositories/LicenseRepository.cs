using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Licensing;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ILicenseRepository.
/// Operates on the single-row license_state table and the consumed_verification_keys table.
/// </summary>
public class LicenseRepository : ILicenseRepository
{
    private readonly ApplicationDbContext _db;

    public LicenseRepository(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Returns the first (and only expected) license row, or null when no license
    /// has been activated yet.
    /// </summary>
    public Task<LicenseState?> GetCurrentAsync(CancellationToken ct = default)
        => _db.LicenseStates.OrderByDescending(l => l.ActivatedAt)
                             .FirstOrDefaultAsync(ct);

    /// <summary>Queues the new license state record for insertion.</summary>
    public async Task AddAsync(LicenseState state, CancellationToken ct = default)
        => await _db.LicenseStates.AddAsync(state, ct);

    /// <summary>Marks the existing license state as modified.</summary>
    public void Update(LicenseState state) => _db.LicenseStates.Update(state);

    /// <summary>
    /// Returns true when the given VerificationKey hash has already been consumed
    /// on this installation.
    /// </summary>
    public Task<bool> IsVerificationKeyConsumedAsync(string keyHash, CancellationToken ct = default)
        => _db.ConsumedVerificationKeys.AnyAsync(k => k.KeyHash == keyHash, ct);

    /// <summary>Queues a new consumed VerificationKey record for insertion.</summary>
    public async Task AddConsumedKeyAsync(ConsumedVerificationKey key, CancellationToken ct = default)
        => await _db.ConsumedVerificationKeys.AddAsync(key, ct);

    /// <summary>Commits pending changes to the database.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
