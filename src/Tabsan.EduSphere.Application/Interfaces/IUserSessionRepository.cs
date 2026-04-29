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
}
