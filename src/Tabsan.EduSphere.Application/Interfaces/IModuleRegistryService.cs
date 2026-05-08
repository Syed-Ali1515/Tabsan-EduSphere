using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Result of a module visibility evaluation for a specific user + institution context.
/// </summary>
public sealed record ModuleVisibilityResult(
    string Key,
    string Name,
    bool   IsActive,
    bool   IsAccessible   // false when role or institution type excludes the module
);

/// <summary>
/// Combines the compile-time <see cref="Tabsan.EduSphere.Application.Modules.ModuleRegistry"/>
/// with live activation state and the current institution policy to answer
/// "which modules should this role see in this tenant?"
/// </summary>
public interface IModuleRegistryService
{
    /// <summary>
    /// Returns all modules with visibility flags resolved for the given role and
    /// institution policy snapshot.
    /// </summary>
    Task<IReadOnlyList<ModuleVisibilityResult>> GetVisibleModulesAsync(
        string role,
        InstitutionPolicySnapshot policy,
        CancellationToken ct = default);

    /// <summary>
    /// Returns true when the module with the given key is both activated and accessible
    /// for the supplied role and institution context.
    /// </summary>
    Task<bool> IsAccessibleAsync(
        string key,
        string role,
        InstitutionPolicySnapshot policy,
        CancellationToken ct = default);
}
