using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserRepository.
/// All database access for the User aggregate goes through this class.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db) => _db = db;

    /// <summary>Finds a user by their GUID primary key. Returns null if not found.</summary>
    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == id, ct);

    /// <summary>
    /// Finds a user by username using a case-insensitive comparison.
    /// Used during login to locate the account before password verification.
    /// </summary>
    public Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        => _db.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), ct);

    /// <summary>Finds a user by email address. Returns null when no match exists.</summary>
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => _db.Users.Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == email.ToLower(), ct);

    /// <summary>Returns true when the username string is already taken by another account.</summary>
    public Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default)
        => _db.Users.AnyAsync(u => u.Username.ToLower() == username.ToLower(), ct);

    /// <summary>Queues the new user entity for insertion on the next SaveChanges call.</summary>
    public async Task AddAsync(User user, CancellationToken ct = default)
        => await _db.Users.AddAsync(user, ct);

    /// <summary>Marks the user entity as Modified so EF Core generates an UPDATE statement.</summary>
    public void Update(User user) => _db.Users.Update(user);

    /// <summary>Flushes all tracked changes to the database in a single transaction.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
