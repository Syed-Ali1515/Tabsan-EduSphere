using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Interfaces;

// ── Value object ──────────────────────────────────────────────────────────────

/// <summary>
/// Immutable snapshot of which institution types are currently enabled.
/// All features that depend on institution mode consume this snapshot.
/// </summary>
public sealed record InstitutionPolicySnapshot(
    bool IncludeSchool,
    bool IncludeCollege,
    bool IncludeUniversity)
{
    /// <summary>True when at least one institution type is enabled.</summary>
    public bool IsValid => IncludeSchool || IncludeCollege || IncludeUniversity;

    /// <summary>Returns true when the specific institution type is currently enabled.</summary>
    public bool IsEnabled(InstitutionType type) => type switch
    {
        InstitutionType.University => IncludeUniversity,
        InstitutionType.School     => IncludeSchool,
        InstitutionType.College    => IncludeCollege,
        _                          => false
    };

    /// <summary>
    /// Default snapshot used when no configuration has been persisted yet.
    /// University-only — fully backward-compatible with all pre-Phase-23 behaviour.
    /// </summary>
    public static InstitutionPolicySnapshot Default { get; } =
        new(IncludeSchool: false, IncludeCollege: false, IncludeUniversity: true);
}

// ── Command ───────────────────────────────────────────────────────────────────

/// <summary>Command carried by the PUT endpoint to update institution type flags.</summary>
public sealed record SaveInstitutionPolicyCommand(
    bool IncludeSchool,
    bool IncludeCollege,
    bool IncludeUniversity);

// ── Service contract ──────────────────────────────────────────────────────────

/// <summary>
/// Centralized service that answers "which institution types are active?" and
/// allows SuperAdmin to change those flags.
/// <para>
/// Values are persisted via <c>portal_settings</c> and held in a 10-minute
/// memory-cache entry so every downstream service pays only one DB round-trip
/// per cache window.
/// </para>
/// </summary>
public interface IInstitutionPolicyService
{
    /// <summary>
    /// Returns the current institution policy snapshot, reading from the cache
    /// first and the database on a miss.
    /// When no flags have been stored yet the default (University-only) snapshot
    /// is returned so the system remains fully functional before any configuration.
    /// </summary>
    Task<InstitutionPolicySnapshot> GetPolicyAsync(CancellationToken ct = default);

    /// <summary>
    /// Persists the supplied flags to <c>portal_settings</c> and invalidates the cache.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when all three flags are <c>false</c> — at least one must be enabled.
    /// </exception>
    Task SavePolicyAsync(SaveInstitutionPolicyCommand cmd, CancellationToken ct = default);

    /// <summary>Evicts the memory-cache entry so the next call reloads from storage.</summary>
    void InvalidateCache();
}
