using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Domain.Modules;

/// <summary>
/// Compile-time specification for a single module: who can use it, under which
/// institution modes, and whether it requires a paid license tier.
/// This is intentionally separate from the DB-backed <see cref="Module"/> /
/// <see cref="ModuleStatus"/> pair that tracks the on/off toggle.
/// </summary>
public sealed record ModuleDescriptor(
    /// <summary>Stable key matching <see cref="Module.Key"/>.</summary>
    string Key,

    /// <summary>
    /// Roles that may access this module.
    /// Empty array means all roles are permitted.
    /// </summary>
    string[] RequiredRoles,

    /// <summary>
    /// Institution types for which this module is available.
    /// Null means the module is available under every institution type.
    /// </summary>
    InstitutionType[]? AllowedTypes = null,

    /// <summary>
    /// When true the module requires an active paid license entitlement
    /// (checked via <see cref="Tabsan.EduSphere.Application.Interfaces.IModuleEntitlementResolver"/>).
    /// </summary>
    bool IsLicenseGated = false
)
{
    /// <summary>Returns true when <paramref name="role"/> satisfies the role requirement.</summary>
    public bool RoleMatches(string role)
        => RequiredRoles.Length == 0
           || RequiredRoles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>Returns true when <paramref name="type"/> is permitted by this descriptor.</summary>
    public bool TypeMatches(InstitutionType type)
        => AllowedTypes is null || AllowedTypes.Contains(type);
}
