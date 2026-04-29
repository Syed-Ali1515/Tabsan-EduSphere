using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository interface for RegistrationWhitelist operations.</summary>
public interface IRegistrationWhitelistRepository
{
    /// <summary>
    /// Looks up an unused whitelist entry by identifier value (case-insensitive).
    /// Returns null when no matching unused entry exists.
    /// </summary>
    Task<RegistrationWhitelist?> FindUnusedAsync(string identifierValue, CancellationToken ct = default);

    /// <summary>Queues a new whitelist entry for insertion.</summary>
    Task AddAsync(RegistrationWhitelist entry, CancellationToken ct = default);

    /// <summary>Queues multiple entries for bulk insertion (Admin imports a cohort CSV).</summary>
    Task AddRangeAsync(IEnumerable<RegistrationWhitelist> entries, CancellationToken ct = default);

    /// <summary>Marks the entry as modified (consumed after registration).</summary>
    void Update(RegistrationWhitelist entry);

    /// <summary>Commits changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
