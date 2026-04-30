using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IPasswordHistoryRepository.
/// Stores and retrieves hashed previous passwords for reuse-prevention policy.
/// </summary>
public class PasswordHistoryRepository : IPasswordHistoryRepository
{
    private readonly ApplicationDbContext _db;

    public PasswordHistoryRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IList<PasswordHistoryEntry>> GetRecentAsync(Guid userId, int count, CancellationToken ct = default)
    {
        return await _db.PasswordHistory
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(PasswordHistoryEntry entry, CancellationToken ct = default)
        => await _db.PasswordHistory.AddAsync(entry, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
