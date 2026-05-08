using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Modules;

/// <summary>
/// Resolves module visibility by combining:
///   1. The static <see cref="ModuleRegistry"/> (role + institution-type constraints).
///   2. Live activation status from <see cref="IModuleEntitlementResolver"/> (cache-backed).
///   3. The current <see cref="InstitutionPolicySnapshot"/>.
/// </summary>
public sealed class ModuleRegistryService : IModuleRegistryService
{
    private readonly IModuleEntitlementResolver _entitlement;
    private readonly IModuleService             _modules;

    public ModuleRegistryService(
        IModuleEntitlementResolver entitlement,
        IModuleService modules)
    {
        _entitlement = entitlement;
        _modules     = modules;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ModuleVisibilityResult>> GetVisibleModulesAsync(
        string role,
        InstitutionPolicySnapshot policy,
        CancellationToken ct = default)
    {
        // Load all live modules (includes Name for display)
        var liveModules = await _modules.GetAllAsync(ct);
        var liveIndex   = liveModules.ToDictionary(m => m.Key, StringComparer.OrdinalIgnoreCase);

        var results = new List<ModuleVisibilityResult>();

        foreach (var descriptor in ModuleRegistry.All())
        {
            liveIndex.TryGetValue(descriptor.Key, out var live);

            // Determine the active institution type(s) from the policy
            var isAccessible = descriptor.RoleMatches(role)
                               && AnyTypeMatches(descriptor, policy);

            var isActive = await _entitlement.IsActiveAsync(descriptor.Key, ct);

            results.Add(new ModuleVisibilityResult(
                Key:          descriptor.Key,
                Name:         live?.Name ?? descriptor.Key,
                IsActive:     isActive,
                IsAccessible: isAccessible));
        }

        return results.OrderBy(r => r.Key).ToList().AsReadOnly();
    }

    /// <inheritdoc/>
    public async Task<bool> IsAccessibleAsync(
        string key,
        string role,
        InstitutionPolicySnapshot policy,
        CancellationToken ct = default)
    {
        var descriptor = ModuleRegistry.Get(key);
        if (descriptor is null) return false;

        return descriptor.RoleMatches(role)
               && AnyTypeMatches(descriptor, policy)
               && await _entitlement.IsActiveAsync(key, ct);
    }

    // ── helpers ──────────────────────────────────────────────────────────────────

    private static bool AnyTypeMatches(
        Domain.Modules.ModuleDescriptor descriptor,
        InstitutionPolicySnapshot policy)
    {
        if (descriptor.AllowedTypes is null) return true;

        foreach (var t in descriptor.AllowedTypes)
        {
            if (policy.IsEnabled(t)) return true;
        }
        return false;
    }
}
