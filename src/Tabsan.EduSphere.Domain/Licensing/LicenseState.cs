using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Licensing;

/// <summary>
/// Stores the currently activated license state on the application side.
/// This table holds only the validated result of reading the signed license file —
/// the raw license key is never stored here; only its SHA-256 hash is kept
/// so we can detect if the file was replaced between validation runs.
///
/// There is always exactly ONE row in this table (the active license).
/// </summary>
public class LicenseState : BaseEntity
{
    /// <summary>
    /// SHA-256 hash of the license file content at the time of last successful validation.
    /// Used on subsequent startups to detect if the file has been swapped or tampered with.
    /// </summary>
    public string LicenseHash { get; private set; } = default!;

    /// <summary>The decoded license type extracted from the verified payload.</summary>
    public LicenseType LicenseType { get; private set; }

    /// <summary>Current computed status, updated by the LicenseValidationService on each check.</summary>
    public LicenseStatus Status { get; private set; }

    /// <summary>UTC timestamp when the license was first activated on this installation.</summary>
    public DateTime ActivatedAt { get; private set; }

    /// <summary>
    /// UTC expiry extracted from the license payload.
    /// Null for Permanent licenses — they never expire.
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    // ── P2-S1-01 / P2-S2-01: Concurrency limit ──────────────────────────────

    /// <summary>
    /// Maximum number of concurrent active sessions allowed.
    /// A value of 0 means unlimited (All Users mode — P2-S2-01).
    /// SuperAdmin is always exempt regardless of this value (P2-S1-02).
    /// </summary>
    public int MaxUsers { get; private set; }

    // ── P2-S3-01 / P2-S3-02: Domain binding ─────────────────────────────────

    /// <summary>
    /// The HTTP host (domain) on which this license was first activated.
    /// Null until first activation. On subsequent activations, the incoming
    /// request host must match this value to prevent reuse across deployments.
    /// </summary>
    public string? ActivatedDomain { get; private set; }

    private LicenseState() { }

    /// <summary>Creates the initial license state record after a successful upload and validation.</summary>
    public LicenseState(string licenseHash, LicenseType licenseType, DateTime? expiresAt,
                        int maxUsers = 0, string? activatedDomain = null)
    {
        LicenseHash = licenseHash;
        LicenseType = licenseType;
        Status = LicenseStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        MaxUsers = maxUsers;
        ActivatedDomain = activatedDomain;
    }

    /// <summary>
    /// Re-evaluates the status based on the current UTC time.
    /// Called during startup validation, daily background checks, and Super Admin login.
    /// </summary>
    public void RefreshStatus()
    {
        if (ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value)
            Status = LicenseStatus.Expired;
        else
            Status = LicenseStatus.Active;

        Touch();
    }

    /// <summary>Forces the status to Invalid when the signature check fails.</summary>
    public void MarkInvalid()
    {
        Status = LicenseStatus.Invalid;
        Touch();
    }

    /// <summary>
    /// Replaces the current license record with a newly uploaded and validated license.
    /// Used when a Super Admin uploads a renewal or upgraded license file.
    /// </summary>
    public void Replace(string newHash, LicenseType newType, DateTime? newExpiry,
                        int maxUsers = 0, string? activatedDomain = null)
    {
        LicenseHash = newHash;
        LicenseType = newType;
        ExpiresAt = newExpiry;
        Status = LicenseStatus.Active;
        ActivatedAt = DateTime.UtcNow;
        MaxUsers = maxUsers;
        ActivatedDomain = activatedDomain;
        Touch();
    }
}
