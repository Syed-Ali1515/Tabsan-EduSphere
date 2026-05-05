using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Auth;

/// <summary>
/// OWASP-recommended Argon2id password hasher.
/// Parameters follow RFC 9106 minimum recommendations:
///   memory = 64 MB, iterations = 3, parallelism = 4, hash length = 32 bytes.
///
/// Hash format (stored in database):
///   argon2id:{base64(salt)}:{base64(hash)}
///
/// Backwards compatibility: hashes produced by the legacy
/// <see cref="PasswordHasher"/> (ASP.NET Identity PBKDF2/HMACSHA512 v3)
/// can still be verified; they are detected by the absence of the
/// "argon2id:" prefix. This allows existing accounts to verify normally
/// and be transparently re-hashed on next login.
/// </summary>
public sealed class Argon2idPasswordHasher : IPasswordHasher
{
    // Argon2id parameters — RFC 9106 "minimum" profile
    private const int SaltBytes       = 32;
    private const int HashBytes       = 32;
    private const int Iterations      = 3;
    private const int MemoryKilobytes = 65536; // 64 MB
    private const int Parallelism     = 4;
    private const string Prefix       = "argon2id:";

    // Legacy PBKDF2 hasher used only for verifying pre-Phase-10 hashes.
    private static readonly Microsoft.AspNetCore.Identity.PasswordHasher<string>
        _legacyHasher = new();

    // ── Hash ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Produces a new Argon2id hash for <paramref name="password"/>.
    /// Always uses a fresh 32-byte cryptographically random salt.
    /// </summary>
    public string Hash(string password)
    {
        var salt       = RandomNumberGenerator.GetBytes(SaltBytes);
        var hashBytes  = ComputeArgon2id(password, salt);

        return $"{Prefix}{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hashBytes)}";
    }

    // ── Verify ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Verifies <paramref name="providedPassword"/> against <paramref name="storedHash"/>.
    /// Supports both Argon2id (new) and legacy PBKDF2 (old) formats.
    /// </summary>
    public bool Verify(string storedHash, string providedPassword)
    {
        if (storedHash.StartsWith(Prefix, StringComparison.Ordinal))
            return VerifyArgon2id(storedHash, providedPassword);

        // Fall back to legacy PBKDF2 verification for pre-Phase-10 accounts.
        return VerifyLegacy(storedHash, providedPassword);
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private static byte[] ComputeArgon2id(string password, byte[] salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password);

        using var argon2 = new Argon2id(passwordBytes)
        {
            Salt                = salt,
            DegreeOfParallelism = Parallelism,
            MemorySize          = MemoryKilobytes,
            Iterations          = Iterations,
        };

        return argon2.GetBytes(HashBytes);
    }

    private static bool VerifyArgon2id(string storedHash, string providedPassword)
    {
        // Format: argon2id:{base64 salt}:{base64 hash}
        var withoutPrefix = storedHash[Prefix.Length..];
        var colonIndex    = withoutPrefix.IndexOf(':');
        if (colonIndex < 0) return false;

        byte[] salt;
        byte[] expectedHash;

        try
        {
            salt         = Convert.FromBase64String(withoutPrefix[..colonIndex]);
            expectedHash = Convert.FromBase64String(withoutPrefix[(colonIndex + 1)..]);
        }
        catch (FormatException)
        {
            return false;
        }

        var actualHash = ComputeArgon2id(providedPassword, salt);

        // Constant-time comparison to prevent timing attacks.
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    private static bool VerifyLegacy(string storedHash, string providedPassword)
    {
        try
        {
            var result = _legacyHasher.VerifyHashedPassword(string.Empty, storedHash, providedPassword);
            return result != Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
