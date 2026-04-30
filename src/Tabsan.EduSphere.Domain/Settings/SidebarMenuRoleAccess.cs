using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Settings;

/// <summary>
/// Maps which roles are allowed to see a specific sidebar menu item.
/// Super Admin bypasses this table and always has full visibility.
/// </summary>
public class SidebarMenuRoleAccess : BaseEntity
{
    /// <summary>FK to the sidebar menu item this access record belongs to.</summary>
    public Guid SidebarMenuItemId { get; private set; }

    /// <summary>Navigation to the parent sidebar menu item.</summary>
    public SidebarMenuItem SidebarMenuItem { get; private set; } = default!;

    /// <summary>
    /// Role name this record applies to (e.g. "Student", "Faculty", "Admin").
    /// Stored as a string to avoid FK coupling to the Roles table.
    /// </summary>
    public string RoleName { get; private set; } = default!;

    /// <summary>True = this role can see the menu item; false = hidden for this role.</summary>
    public bool IsAllowed { get; private set; } = true;

    private SidebarMenuRoleAccess() { }

    public SidebarMenuRoleAccess(Guid sidebarMenuItemId, string roleName, bool isAllowed = true)
    {
        SidebarMenuItemId = sidebarMenuItemId;
        RoleName          = roleName.Trim();
        IsAllowed         = isAllowed;
    }

    /// <summary>Flips the access flag.</summary>
    public void SetAllowed(bool isAllowed) => IsAllowed = isAllowed;
}
