using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IUserSessionRepository.
/// Manages the UserSession rows used for refresh-token tracking.
/// </summary>
public class UserSessionRepository : IUserSessionRepository
{
    private readonly ApplicationDbContext _db;

    public UserSessionRepository(ApplicationDbContext db) => _db = db;

    /// <summary>
    /// Returns the session whose token hash matches AND has not been revoked.
    /// Returns null when no active matching session exists.
    /// </summary>
    public Task<UserSession?> GetActiveByHashAsync(string tokenHash, CancellationToken ct = default)
        => _db.UserSessions
              .FirstOrDefaultAsync(s => s.RefreshTokenHash == tokenHash && s.RevokedAt == null, ct);

    /// <summary>Queues the new session for insertion.</summary>
    public async Task AddAsync(UserSession session, CancellationToken ct = default)
        => await _db.UserSessions.AddAsync(session, ct);

    /// <summary>Marks the session entity as modified.</summary>
    public void Update(UserSession session) => _db.UserSessions.Update(session);

    /// <summary>Commits all pending changes to the database.</summary>
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    // ── P2-S1-01: Concurrency limit ──────────────────────────────────────────

    /// <summary>
    /// Returns the count of sessions that are currently active (not revoked, not expired).
    /// Used to enforce the MaxUsers license limit prior to allowing a new login.
    /// </summary>
    public Task<int> CountActiveSessionsAsync(CancellationToken ct = default)
        => _db.UserSessions
              .CountAsync(s => s.RevokedAt == null && s.ExpiresAt > DateTime.UtcNow, ct);
}
