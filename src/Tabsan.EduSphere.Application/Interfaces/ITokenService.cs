using Tabsan.EduSphere.Domain.Identity;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application-layer contract for generating and validating authentication tokens.
/// The Infrastructure layer (TokenService) implements this — the Application layer
/// depends only on this interface to stay independent of JWT libraries.
/// </summary>
public interface ITokenService
{
    /// <summary>Generates a signed JWT access token for the given user.</summary>
    string GenerateAccessToken(User user);

    /// <summary>Generates a cryptographically random raw refresh token string.</summary>
    string GenerateRefreshToken();

    /// <summary>Computes the SHA-256 hash of a raw refresh token for storage.</summary>
    string HashRefreshToken(string rawToken);

    /// <summary>Returns the UTC expiry date for a new refresh token session.</summary>
    DateTime GetRefreshTokenExpiry();
}
