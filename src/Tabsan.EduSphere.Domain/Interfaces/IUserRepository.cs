using Tabsan.EduSphere.Domain.Identity;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Repository interface for User aggregate operations.
/// The Infrastructure layer provides the EF Core implementation.
/// Using an interface here keeps the Domain and Application layers
/// independent of any specific ORM or database technology.
/// </summary>
public interface IUserRepository
{
    /// <summary>Returns the user with the given ID, or null if not found.</summary>
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the user with the given username (case-insensitive), or null.</summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default);

    /// <summary>Returns the user with the given email address, or null.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);

    /// <summary>Returns true when the username is already taken.</summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);

    /// <summary>Returns all non-admin user accounts that are currently locked out.</summary>
    Task<IList<User>> GetLockedAccountsAsync(CancellationToken ct = default);

    /// <summary>Returns all active users assigned to the Faculty role.</summary>
    Task<IList<User>> GetFacultyUsersAsync(CancellationToken ct = default);

    /// <summary>Returns all active users assigned to any of the provided role names.</summary>
    Task<IList<User>> GetActiveUsersByRolesAsync(IReadOnlyList<string> roleNames, CancellationToken ct = default);

    /// <summary>Returns users assigned to any of the provided role names, optionally including inactive accounts.</summary>
    Task<IList<User>> GetUsersByRolesAsync(IReadOnlyList<string> roleNames, bool includeInactive = false, CancellationToken ct = default);

    /// <summary>Returns the role matching the given name (case-insensitive), or null if not found.</summary>
    Task<Role?> GetRoleByNameAsync(string roleName, CancellationToken ct = default);

    /// <summary>Persists a new user entity.</summary>
    Task AddAsync(User user, CancellationToken ct = default);

    /// <summary>Persists a collection of new user entities in a single batch (P4-S1-01 CSV import).</summary>
    Task AddRangeAsync(IEnumerable<User> users, CancellationToken ct = default);

    /// <summary>Marks the entity as modified so EF Core tracks the change.</summary>
    void Update(User user);

    /// <summary>Flushes all pending changes to the database within the current unit of work.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
