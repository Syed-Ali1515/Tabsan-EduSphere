using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Licensing;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ILicenseRepository.
/// Operates on the single-row license_state table.
/// </summary>
public class LicenseRepository : ILicenseRepository
{
    private readonly ApplicationDbContext _db;

    public LicenseRepository(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Returns the first (and only expected) license row, or null when no license
    /// has been activated yet. Ordered by ActivatedAt descending so the most
    /// recently activated row is returned if multiple rows exist after data migration.
    /// </summary>
    public Task<LicenseState?> GetCurrentAsync(CancellationToken ct = default)
        => _db.LicenseStates.OrderByDescending(l => l.ActivatedAt)
                             .FirstOrDefaultAsync(ct);

    /// <summary>Queues the new license state record for insertion.</summary>
    public async Task AddAsync(LicenseState state, CancellationToken ct = default)
        => await _db.LicenseStates.AddAsync(state, ct);

    /// <summary>Marks the existing license state as modified.</summary>
    public void Update(LicenseState state) => _db.LicenseStates.Update(state);

    /// <summary>Commits pending changes to the database.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
