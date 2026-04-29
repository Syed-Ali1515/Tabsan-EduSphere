using Tabsan.EduSphere.Domain.Common;
using Tabsan.EduSphere.Domain.Modules;

namespace Tabsan.EduSphere.Domain.Settings;

/// <summary>
/// Junction record that maps which roles can access a given module.
/// Super Admin always sees all modules regardless of this table.
/// When a module is inactive, even assigned roles cannot access it.
/// </summary>
public class ModuleRoleAssignment : BaseEntity
{
    /// <summary>FK to the module.</summary>
    public Guid ModuleId { get; private set; }

    /// <summary>Navigation to the module.</summary>
    public Module Module { get; private set; } = default!;

    /// <summary>
    /// Role name this assignment applies to (e.g. "Student", "Faculty", "Admin").
    /// Stored as a string rather than FK to avoid tight coupling to the Roles table.
    /// </summary>
    public string RoleName { get; private set; } = default!;

    private ModuleRoleAssignment() { }

    public ModuleRoleAssignment(Guid moduleId, string roleName)
    {
        ModuleId = moduleId;
        RoleName = roleName.Trim();
    }
}
