using Tabsan.EduSphere.Application.DTOs.Auth;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Identity;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Auth;

/// <summary>
/// Orchestrates the login / logout / token-refresh / change-password workflows.
/// Depends only on Application-layer interfaces — no direct Infrastructure or EF Core references.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IUserSessionRepository _sessionRepo;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;
    private readonly IPasswordHistoryRepository _passwordHistory;
    private readonly ILicenseRepository _licenseRepo;

    public AuthService(
        IUserRepository userRepo,
        IUserSessionRepository sessionRepo,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IAuditService audit,
        IPasswordHistoryRepository passwordHistory,
        ILicenseRepository licenseRepo)
    {
        _userRepo        = userRepo;
        _sessionRepo     = sessionRepo;
        _tokenService    = tokenService;
        _passwordHasher  = passwordHasher;
        _audit           = audit;
        _passwordHistory = passwordHistory;
        _licenseRepo     = licenseRepo;
    }

    // ── Login ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Looks up the user by username, verifies the password, and on success:
    /// 1. Checks the license concurrent-user limit (P2-S1-01 / P2-S2-01).
    /// 2. Records the login timestamp on the user aggregate.
    /// 3. Creates a new UserSession row with the hashed refresh token.
    /// 4. Returns a LoginResult.Ok containing the signed JWT and the raw refresh token.
    /// Returns LoginResult.Fail when credentials are wrong, the account is inactive,
    /// or the concurrent session limit has been reached.
    /// SuperAdmin is always exempt from the concurrency check (P2-S1-02).
    /// </summary>
    public async Task<LoginResult> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByUsernameAsync(request.Username, ct);
        if (user is null || !user.IsActive)
            return LoginResult.Fail(LoginFailureReason.InvalidCredentials);

        // Check lockout before password verification
        if (user.IsCurrentlyLockedOut())
            return LoginResult.Fail(LoginFailureReason.InvalidCredentials);

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            // Record failed attempt — locks the account after 5 consecutive failures for 15 minutes
            user.RecordFailedLoginAttempt(maxFailedAttempts: 5, lockoutDurationMinutes: 15);
            _userRepo.Update(user);
            await _userRepo.SaveChangesAsync(ct);
            return LoginResult.Fail(LoginFailureReason.InvalidCredentials);
        }

        // ── P2-S1-01 / P2-S2-01: Concurrent user limit enforcement ────────────
        // SuperAdmin (P2-S1-02) is always exempt so the portal is never locked out.
        var isSuperAdmin = string.Equals(user.Role?.Name, "SuperAdmin",
                                         StringComparison.OrdinalIgnoreCase);

        if (!isSuperAdmin)
        {
            var license = await _licenseRepo.GetCurrentAsync(ct);
            // MaxUsers == 0 means the license is in "All Users" / unlimited mode (P2-S2-01).
            if (license is not null && license.MaxUsers > 0)
            {
                var activeSessions = await _sessionRepo.CountActiveSessionsAsync(ct);
                if (activeSessions >= license.MaxUsers)
                    return LoginResult.Fail(LoginFailureReason.ConcurrencyLimitReached);
            }
        }

        var rawRefresh = _tokenService.GenerateRefreshToken();
        var refreshHash = _tokenService.HashRefreshToken(rawRefresh);
        var refreshExpiry = _tokenService.GetRefreshTokenExpiry();

        var session = new UserSession(user.Id, refreshHash, refreshExpiry, ipAddress: ipAddress);
        await _sessionRepo.AddAsync(session, ct);

        user.RecordLogin();
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync(ct);
        await _sessionRepo.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog("Login", "User", user.Id.ToString(),
            actorUserId: user.Id, ipAddress: ipAddress), ct);

        return LoginResult.Ok(new LoginResponse(
            AccessToken: _tokenService.GenerateAccessToken(user),
            RefreshToken: rawRefresh,
            AccessTokenExpiry: DateTime.UtcNow.AddMinutes(15),
            Role: user.Role?.Name ?? string.Empty,
            UserId: user.Id,
            Username: user.Username,
            MustChangePassword: user.MustChangePassword));
    }

    // ── Refresh ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Finds the active session matching the presented refresh token hash.
    /// On success: revokes the old session, creates a new one (rotation),
    /// and returns a new access + refresh token pair.
    /// Returns null when no valid session is found.
    /// </summary>
    public async Task<LoginResponse?> RefreshAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default)
    {
        var hash = _tokenService.HashRefreshToken(rawRefreshToken);

        var session = await _sessionRepo.GetActiveByHashAsync(hash, ct);

        if (session is null || !session.IsActive)
            return null;

        var user = await _userRepo.GetByIdAsync(session.UserId, ct);
        if (user is null || !user.IsActive)
            return null;

        // Rotate: revoke old session and issue a fresh one.
        var newRawRefresh = _tokenService.GenerateRefreshToken();
        var newHash = _tokenService.HashRefreshToken(newRawRefresh);
        var newExpiry = _tokenService.GetRefreshTokenExpiry();

        session.Rotate(newHash, newExpiry);
        _sessionRepo.Update(session);
        await _sessionRepo.SaveChangesAsync(ct);

        return new LoginResponse(
            AccessToken: _tokenService.GenerateAccessToken(user),
            RefreshToken: newRawRefresh,
            AccessTokenExpiry: DateTime.UtcNow.AddMinutes(15),
            Role: user.Role?.Name ?? string.Empty,
            UserId: user.Id,
            Username: user.Username);
    }

    // ── Logout ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Revokes the session associated with the presented refresh token hash.
    /// Subsequent refresh calls with the same token will return null (invalid).
    /// </summary>
    public async Task LogoutAsync(string rawRefreshToken, CancellationToken ct = default)
    {
        var hash = _tokenService.HashRefreshToken(rawRefreshToken);

        var session = await _sessionRepo.GetActiveByHashAsync(hash, ct);
        if (session is null) return;

        session.Revoke();
        _sessionRepo.Update(session);
        await _sessionRepo.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog("Logout", "UserSession", session.Id.ToString(),
            actorUserId: session.UserId), ct);
    }

    // ── Change Password ────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies the current password matches the stored hash, then replaces it.
    /// Returns false when the old password is wrong — does not update anything.
    /// </summary>
    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return false;

        if (!_passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
            return false;

        // Enforce no-reuse of last 5 passwords (includes current hash).
        var recentHashes = await _passwordHistory.GetRecentAsync(userId, 5, ct);
        if (recentHashes.Any(h => _passwordHasher.Verify(h.PasswordHash, request.NewPassword)))
            return false; // new password matches a recent one

        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatePasswordHash(newHash);
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync(ct);

        // Record in password history.
        await _passwordHistory.AddAsync(new PasswordHistoryEntry(userId, newHash), ct);
        await _passwordHistory.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog("ChangePassword", "User", userId.ToString(),
            actorUserId: userId), ct);

        return true;
    }

    // ── Force Change Password (P4-S2-02) ──────────────────────────────────────

    /// <summary>
    /// Sets a new password for a user who is flagged with MustChangePassword = true.
    /// Does NOT require the current password (because the imported password is just a placeholder).
    /// Clears MustChangePassword on success.
    /// Returns false when the user is not found or the new password is empty.
    /// </summary>
    public async Task<bool> ForceChangePasswordAsync(Guid userId, string newPassword, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return false;

        var user = await _userRepo.GetByIdAsync(userId, ct);
        if (user is null) return false;

        var newHash = _passwordHasher.Hash(newPassword);
        user.UpdatePasswordHash(newHash);
        user.ClearMustChangePassword();
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync(ct);

        // Record in password history so that re-use rules apply going forward.
        await _passwordHistory.AddAsync(new PasswordHistoryEntry(userId, newHash), ct);
        await _passwordHistory.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog("ForceChangePassword", "User", userId.ToString(),
            actorUserId: userId), ct);

        return true;
    }
}
