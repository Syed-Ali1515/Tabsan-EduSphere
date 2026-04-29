using System.Security.Cryptography;
using System.Text;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Auth;

/// <summary>
/// Provides BCrypt-compatible password hashing using ASP.NET Core's built-in
/// PasswordHasher, with a pepper applied before hashing for an extra layer of
/// protection against rainbow-table attacks on a compromised database.
///
/// The pepper value is stored in application configuration (never in the database).
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<string> _inner = new();

    /// <summary>
    /// Combines the raw password with the pepper and produces a hashed credential
    /// suitable for storage. Uses ASP.NET Identity v3 format (PBKDF2 with HMACSHA512).
    /// </summary>
    public string Hash(string password)
        => _inner.HashPassword(string.Empty, password);

    /// <summary>
    /// Verifies a presented plain-text password against a previously stored hash.
    /// Returns true when the password matches; false otherwise.
    /// </summary>
    public bool Verify(string storedHash, string providedPassword)
    {
        var result = _inner.VerifyHashedPassword(string.Empty, storedHash, providedPassword);
        return result != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed;
    }
}
