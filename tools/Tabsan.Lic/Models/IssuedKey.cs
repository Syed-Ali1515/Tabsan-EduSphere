namespace Tabsan.Lic.Models;

/// <summary>
/// Represents a VerificationKey issued by Tabsan-Lic and stored in the local SQLite database.
/// Only the SHA-256 hash of the raw key is persisted; the raw key is shown once on generation.
/// </summary>
public class IssuedKey
{
    /// <summary>Surrogate primary key (auto-increment).</summary>
    public int Id { get; set; }

    /// <summary>
    /// Stable unique identifier for this issued key.
    /// This Guid is embedded in the .tablic payload as the VerificationKeyId for
    /// operator reference; EduSphere stores the hash, not this Guid.
    /// </summary>
    public Guid KeyId { get; set; }

    /// <summary>
    /// SHA-256 hex-string of the raw VerificationKey token.
    /// Matches the value embedded in the .tablic payload's VerificationKeyHash field.
    /// </summary>
    public string VerificationKeyHash { get; set; } = default!;

    /// <summary>The expiry category selected when this key was generated.</summary>
    public ExpiryType ExpiryType { get; set; }

    /// <summary>UTC timestamp when this key was created in Tabsan-Lic.</summary>
    public DateTime IssuedAt { get; set; }

    /// <summary>
    /// UTC expiry date embedded in the corresponding .tablic payload.
    /// Null for Permanent licenses.
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>True once a .tablic file has been generated for this key record.</summary>
    public bool IsLicenseGenerated { get; set; }

    /// <summary>UTC timestamp when the .tablic file was generated. Null if not yet generated.</summary>
    public DateTime? LicenseGeneratedAt { get; set; }

    /// <summary>Human-readable label / customer note (optional).</summary>
    public string? Label { get; set; }
}
