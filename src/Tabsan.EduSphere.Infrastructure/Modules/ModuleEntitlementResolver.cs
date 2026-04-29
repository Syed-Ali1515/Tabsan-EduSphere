using Microsoft.Extensions.Caching.Memory;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Modules;

/// <summary>
/// Resolves whether a named module is currently active by combining the database
/// module status with an in-memory cache to avoid hitting the DB on every request.
///
/// Cache entries expire after 60 seconds so module toggles propagate quickly
/// without requiring a restart. Super Admin module changes invalidate the cache
/// immediately via InvalidateCache().
/// </summary>
public class ModuleEntitlementResolver : IModuleEntitlementResolver
{
    private readonly IModuleRepository _moduleRepo;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);
    private const string CacheKeyPrefix = "module_active_";

    public ModuleEntitlementResolver(IModuleRepository moduleRepo, IMemoryCache cache)
    {
        _moduleRepo = moduleRepo;
        _cache = cache;
    }

    /// <summary>
    /// Returns true when the named module is active.
    /// Result is cached for 60 seconds to reduce database load on high-traffic endpoints.
    /// </summary>
    public async Task<bool> IsActiveAsync(string moduleKey, CancellationToken ct = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{moduleKey}";

        if (_cache.TryGetValue(cacheKey, out bool cached))
            return cached;

        var isActive = await _moduleRepo.IsActiveAsync(moduleKey, ct);
        _cache.Set(cacheKey, isActive, CacheTtl);
        return isActive;
    }

    /// <summary>
    /// Removes the cached value for a specific module so the next request
    /// reads the latest state from the database.
    /// Called after a Super Admin toggles a module on or off.
    /// </summary>
    public void InvalidateCache(string moduleKey)
        => _cache.Remove($"{CacheKeyPrefix}{moduleKey}");

    /// <summary>
    /// Clears all module entitlement cache entries.
    /// Used after bulk module changes or license updates.
    /// </summary>
    public void InvalidateAll()
    {
        // IMemoryCache does not support prefix-based invalidation natively.
        // We dispose and recreate the entries by calling Remove on known keys.
        // For a larger module set a dedicated cache region or MemoryCacheEntryOptions tag would be used.
        foreach (var key in KnownModuleKeys.All)
            _cache.Remove($"{CacheKeyPrefix}{key}");
    }
}

/// <summary>
/// Centralised list of module key constants.
/// Referenced in policy filters, service guards, and entitlement checks
/// so string literals never appear in more than one place.
/// </summary>
public static class KnownModuleKeys
{
    public const string Authentication = "authentication";
    public const string Departments    = "departments";
    public const string Sis            = "sis";
    public const string Courses        = "courses";
    public const string Assignments    = "assignments";
    public const string Quizzes        = "quizzes";
    public const string Attendance     = "attendance";
    public const string Results        = "results";
    public const string Notifications  = "notifications";
    public const string Fyp            = "fyp";
    public const string AiChat         = "ai_chat";
    public const string Reports        = "reports";
    public const string Themes         = "themes";
    public const string AdvancedAudit  = "advanced_audit";

    /// <summary>All known keys — used for bulk cache invalidation.</summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        Authentication, Departments, Sis, Courses, Assignments,
        Quizzes, Attendance, Results, Notifications, Fyp,
        AiChat, Reports, Themes, AdvancedAudit
    };
}
