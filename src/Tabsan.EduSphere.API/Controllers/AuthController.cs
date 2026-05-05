using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Auth;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Handles all authentication operations: login, logout, token refresh, and password change.
/// Refresh tokens are exchanged as JSON body values (callers should store them securely).
/// All endpoints are under /api/v1/auth.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    // ── POST /api/v1/auth/login ────────────────────────────────────────────────

    /// <summary>
    /// Authenticates the user and returns a JWT access token plus a refresh token.
    /// Returns 401 Unauthorized when credentials are invalid or the account is inactive.
    /// Returns 403 Forbidden when the license concurrent-user limit has been reached (P2-S1-01).
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        // Capture the client IP for audit logging.
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _auth.LoginAsync(request, ip, ct);

        if (!result.IsSuccess)
        {
            // P2-S1-01: Return 403 so the client can distinguish limit-reached from bad credentials.
            if (result.FailureReason == LoginFailureReason.ConcurrencyLimitReached)
                return StatusCode(403, new { message = "Login limit reached. The maximum number of concurrent users allowed by the current license has been reached. Please contact your administrator." });

            return Unauthorized(new { message = "Invalid credentials or account inactive." });
        }

        return Ok(result.Response);
    }

    // ── POST /api/v1/auth/refresh ──────────────────────────────────────────────

    /// <summary>
    /// Rotates the presented refresh token and returns a new access + refresh pair.
    /// Returns 401 when the token is invalid, expired, or already revoked.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _auth.RefreshAsync(request.RefreshToken, ip, ct);

        if (result is null)
            return Unauthorized(new { message = "Invalid or expired refresh token." });

        return Ok(result);
    }

    // ── POST /api/v1/auth/logout ───────────────────────────────────────────────

    /// <summary>
    /// Revokes the current session associated with the presented refresh token.
    /// Always returns 204 No Content — even when the token does not exist —
    /// to avoid revealing session state to callers.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] RefreshRequest request, CancellationToken ct)
    {
        await _auth.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    // ── PUT /api/v1/auth/change-password ──────────────────────────────────────

    /// <summary>
    /// Changes the authenticated user's password.
    /// Returns 400 Bad Request when the current password is wrong.
    /// </summary>
    [HttpPut("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
    {
        // Read the user ID from the JWT sub claim.
        var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userIdStr, out var userId))
            return Forbid();

        var success = await _auth.ChangePasswordAsync(userId, request, ct);

        if (!success)
            return BadRequest(new { message = "Current password is incorrect." });

        return NoContent();
    }
}
