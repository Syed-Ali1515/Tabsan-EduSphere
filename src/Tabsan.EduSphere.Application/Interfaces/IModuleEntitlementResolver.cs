namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application-layer contract for querying module entitlement state.
/// Backed by ModuleEntitlementResolver in Infrastructure which uses a memory cache.
/// </summary>
public interface IModuleEntitlementResolver
{
    /// <summary>Returns true when the named module is currently active.</summary>
    Task<bool> IsActiveAsync(string moduleKey, CancellationToken ct = default);

    /// <summary>Invalidates the cache entry for a single module key after a toggle.</summary>
    void InvalidateCache(string moduleKey);
}
