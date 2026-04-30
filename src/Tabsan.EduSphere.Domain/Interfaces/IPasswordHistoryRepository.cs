using Tabsan.EduSphere.Domain.Identity;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Repository for reading and writing password history entries.
/// Used to enforce the "cannot reuse last 5 passwords" policy.
/// </summary>
public interface IPasswordHistoryRepository
{
    /// <summary>Returns the N most recent history entries for a user, newest first.</summary>
    Task<IList<PasswordHistoryEntry>> GetRecentAsync(Guid userId, int count, CancellationToken ct = default);

    /// <summary>Persists a new history entry (call after the password hash has been updated on the User).</summary>
    Task AddAsync(PasswordHistoryEntry entry, CancellationToken ct = default);

    /// <summary>Commits pending changes to the database.</summary>
    Task SaveChangesAsync(CancellationToken ct = default);
}
