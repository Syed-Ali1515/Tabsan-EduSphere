using Tabsan.EduSphere.Domain.Identity;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application-layer contract for reading active user sessions (refresh tokens).
/// Keeps AuthService independent of EF Core DbContext directly.
/// </summary>
public interface IUserSessionRepository
{
    /// <summary>Returns the active session matching the token hash, or null.</summary>
    Task<UserSession?> GetActiveByHashAsync(string tokenHash, CancellationToken ct = default);

    /// <summary>Persists a new session.</summary>
    Task AddAsync(UserSession session, CancellationToken ct = default);

    /// <summary>Marks a session as modified.</summary>
    void Update(UserSession session);

    /// <summary>Commits pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    // ── P2-S1-01: Concurrency limit ──────────────────────────────────────────

    /// <summary>
    /// Returns the number of sessions that are currently active across ALL users
    /// (RevokedAt is null AND ExpiresAt is in the future).
    /// Used to enforce the MaxUsers license limit before allowing a new login.
    /// </summary>
    Task<int> CountActiveSessionsAsync(CancellationToken ct = default);
}
