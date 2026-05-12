using Microsoft.Extensions.Caching.Memory;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Phase 23 Stage 23.1 — License Policy Kernel.
/// Reads institution-type flags from <c>portal_settings</c>, caches the
/// resolved <see cref="InstitutionPolicySnapshot"/> for 10 minutes, and
/// provides the single write path that SuperAdmin uses to update flags.
/// </summary>
public sealed class InstitutionPolicyService : IInstitutionPolicyService
{
    private const string KeySchool     = "institution_include_school";
    private const string KeyCollege    = "institution_include_college";
    private const string KeyUniversity = "institution_include_university";
    private const string CacheKey      = "institution_policy_snapshot";
    private static readonly TimeSpan   CacheTtl = TimeSpan.FromMinutes(10);

    private readonly ISettingsRepository _settings;
    private readonly IMemoryCache        _cache;

    public InstitutionPolicyService(ISettingsRepository settings, IMemoryCache cache)
    {
        _settings = settings;
        _cache    = cache;
    }

    /// <inheritdoc/>
    public async Task<InstitutionPolicySnapshot> GetPolicyAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey, out InstitutionPolicySnapshot? cached) && cached is not null)
            return cached;

        // Read all portal settings in one DB round-trip.
        var all = await _settings.GetAllPortalSettingsAsync();

        var snapshot = new InstitutionPolicySnapshot(
            IncludeSchool:     ParseFlag(all, KeySchool,     defaultValue: false),
            IncludeCollege:    ParseFlag(all, KeyCollege,    defaultValue: false),
            IncludeUniversity: ParseFlag(all, KeyUniversity, defaultValue: true) // backward-compat default
        );

        // Protect against an edge case where all flags were somehow saved as false.
        var result = snapshot.IsValid ? snapshot : InstitutionPolicySnapshot.Default;

        _cache.Set(CacheKey, result, CacheTtl);
        return result;
    }

    /// <inheritdoc/>
    public async Task SavePolicyAsync(SaveInstitutionPolicyCommand cmd, CancellationToken ct = default)
    {
        if (!cmd.IncludeSchool && !cmd.IncludeCollege && !cmd.IncludeUniversity)
            throw new InvalidOperationException(
                "At least one institution type (School, College, or University) must be enabled.");

        await _settings.UpsertPortalSettingAsync(KeySchool,     BoolStr(cmd.IncludeSchool),     ct);
        await _settings.UpsertPortalSettingAsync(KeyCollege,    BoolStr(cmd.IncludeCollege),    ct);
        await _settings.UpsertPortalSettingAsync(KeyUniversity, BoolStr(cmd.IncludeUniversity), ct);
        await _settings.SaveChangesAsync(ct);

        InvalidateCache();
    }

    /// <inheritdoc/>
    public void InvalidateCache() => _cache.Remove(CacheKey);

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static bool ParseFlag(Dictionary<string, string> settings, string key, bool defaultValue)
    {
        if (settings.TryGetValue(key, out var raw) && bool.TryParse(raw, out var parsed))
            return parsed;
        return defaultValue;
    }

    private static string BoolStr(bool value) => value ? "true" : "false";
}
