namespace Tabsan.EduSphere.Application.DTOs.Auth;

/// <summary>Request body sent by a client to the POST /api/v1/auth/login endpoint.</summary>
public sealed record LoginRequest(string Username, string Password);

/// <summary>
/// Returned on a successful login.
/// The access token is short-lived (default 15 min).
/// The refresh token is long-lived (default 7 days) and stored as an HttpOnly cookie.
/// </summary>
public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    string Role,
    Guid UserId,
    string Username);

/// <summary>Request body sent to POST /api/v1/auth/refresh.</summary>
public sealed record RefreshRequest(string RefreshToken);

/// <summary>Request body for POST /api/v1/auth/change-password.</summary>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
