using Tabsan.EduSphere.Application.DTOs.Auth;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Contract for authentication operations executed by the Application layer.
/// The Infrastructure layer provides token and hashing services;
/// this interface orchestrates them without depending on any specific implementation.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Validates credentials and, on success, creates a new session returning
    /// an access token and a refresh token.
    /// Returns a LoginResult with IsSuccess=false when credentials are invalid,
    /// the account is inactive, or the license concurrency limit has been reached (P2-S1-01).
    /// </summary>
    Task<LoginResult> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct = default);

    /// <summary>
    /// Validates the presented refresh token, rotates it (old token revoked, new issued),
    /// and returns a new access + refresh token pair.
    /// Returns null when the token is invalid, expired, or already revoked.
    /// </summary>
    Task<LoginResponse?> RefreshAsync(string rawRefreshToken, string? ipAddress, CancellationToken ct = default);

    /// <summary>
    /// Revokes the session associated with the presented refresh token.
    /// Used by the logout endpoint.
    /// </summary>
    Task LogoutAsync(string rawRefreshToken, CancellationToken ct = default);

    /// <summary>
    /// Verifies the current password and replaces it with the new one.
    /// Returns false when the current password does not match.
    /// </summary>
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
}
