using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Settings;

/// <summary>
/// Represents a single entry in the sidebar navigation (either a top-level menu or a sub-menu item).
/// Super Admin always sees every item regardless of IsActive or role assignments.
/// </summary>
public class SidebarMenuItem : AuditableEntity
{
    /// <summary>Display label shown in the sidebar (e.g. "Manage Timetables").</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Short description of the menu's purpose, shown in Sidebar Settings table.</summary>
    public string Purpose { get; private set; } = default!;

    /// <summary>
    /// Stable string key identifying this menu in code (e.g. "timetable_admin").
    /// Never changed after seeding.
    /// </summary>
    public string Key { get; private set; } = default!;

    /// <summary>
    /// FK to the parent menu item. Null means this is a top-level menu.
    /// Non-null means this is a sub-menu entry under the specified parent.
    /// </summary>
    public Guid? ParentId { get; private set; }

    /// <summary>Navigation property to the parent menu item.</summary>
    public SidebarMenuItem? Parent { get; private set; }

    /// <summary>Collection of direct child sub-menu items.</summary>
    public ICollection<SidebarMenuItem> SubMenus { get; private set; } = new List<SidebarMenuItem>();

    /// <summary>Controls sort order within the same level (parent group).</summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// When false the item is hidden from ALL roles except Super Admin.
    /// Super Admin always sees the item regardless of this flag.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// System menus (e.g. License, Module Settings, Sidebar Settings) that cannot be deactivated for Super Admin.
    /// These can still be restricted for other roles.
    /// </summary>
    public bool IsSystemMenu { get; private set; }

    /// <summary>Role access assignments for this menu item.</summary>
    public ICollection<SidebarMenuRoleAccess> RoleAccesses { get; private set; } = new List<SidebarMenuRoleAccess>();

    private SidebarMenuItem() { }

    public SidebarMenuItem(string key, string name, string purpose, int displayOrder, Guid? parentId = null, bool isSystemMenu = false)
    {
        Key          = key.Trim().ToLowerInvariant();
        Name         = name.Trim();
        Purpose      = purpose.Trim();
        DisplayOrder = displayOrder;
        ParentId     = parentId;
        IsSystemMenu = isSystemMenu;
        IsActive     = true;
    }

    /// <summary>Activates the menu so it is visible to all assigned roles.</summary>
    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    /// <summary>
    /// Deactivates the menu — hidden from all roles except Super Admin.
    /// System menus cannot be deactivated: throws if <see cref="IsSystemMenu"/> is true.
    /// </summary>
    public void Deactivate()
    {
        if (IsSystemMenu)
            throw new InvalidOperationException($"System menu '{Name}' cannot be deactivated.");

        IsActive = false;
        Touch();
    }

    public void Update(string name, string purpose, int displayOrder)
    {
        Name         = name.Trim();
        Purpose      = purpose.Trim();
        DisplayOrder = displayOrder;
        Touch();
    }
}
