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

    public AuthService(
        IUserRepository userRepo,
        IUserSessionRepository sessionRepo,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IAuditService audit)
    {
        _userRepo = userRepo;
        _sessionRepo = sessionRepo;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _audit = audit;
    }

    // ── Login ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Looks up the user by username, verifies the password, and on success:
    /// 1. Records the login timestamp on the user aggregate.
    /// 2. Creates a new UserSession row with the hashed refresh token.
    /// 3. Returns a LoginResponse containing the signed JWT and the raw refresh token.
    /// Returns null when credentials are wrong or the account is inactive.
    /// </summary>
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByUsernameAsync(request.Username, ct);
        if (user is null || !user.IsActive)
            return null;

        if (!_passwordHasher.Verify(user.PasswordHash, request.Password))
            return null;

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

        return new LoginResponse(
            AccessToken: _tokenService.GenerateAccessToken(user),
            RefreshToken: rawRefresh,
            AccessTokenExpiry: DateTime.UtcNow.AddMinutes(15),
            Role: user.Role?.Name ?? string.Empty,
            UserId: user.Id,
            Username: user.Username);
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

        var newHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatePasswordHash(newHash);
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog("ChangePassword", "User", userId.ToString(),
            actorUserId: userId), ct);

        return true;
    }
}
