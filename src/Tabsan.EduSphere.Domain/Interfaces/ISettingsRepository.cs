using Tabsan.EduSphere.Domain.Settings;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>
/// Repository contract for Report Settings and Module Role Assignment operations.
/// </summary>
public interface ISettingsRepository
{
    // ── Report Definitions ────────────────────────────────────────────────
    /// <summary>Returns all report definitions with their role assignments loaded.</summary>
    Task<IList<ReportDefinition>> GetAllReportsAsync(CancellationToken ct = default);

    /// <summary>Returns a report definition by its stable key (e.g. "attendance_summary").</summary>
    Task<ReportDefinition?> GetReportByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>Returns a report definition by its ID with role assignments loaded.</summary>
    Task<ReportDefinition?> GetReportByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Creates a new report definition.</summary>
    Task AddReportAsync(ReportDefinition report, CancellationToken ct = default);

    /// <summary>Marks a report definition as modified.</summary>
    void UpdateReport(ReportDefinition report);

    // ── Report Role Assignments ───────────────────────────────────────────
    /// <summary>Adds a role assignment to a report.</summary>
    Task AddReportRoleAsync(ReportRoleAssignment assignment, CancellationToken ct = default);

    /// <summary>Removes a role assignment from a report.</summary>
    void RemoveReportRole(ReportRoleAssignment assignment);

    /// <summary>Returns a specific report role assignment, or null.</summary>
    Task<ReportRoleAssignment?> GetReportRoleAsync(Guid reportId, string roleName, CancellationToken ct = default);

    // ── Module Role Assignments ───────────────────────────────────────────
    /// <summary>Returns all role assignments for a module (by module ID).</summary>
    Task<IList<ModuleRoleAssignment>> GetModuleRolesAsync(Guid moduleId, CancellationToken ct = default);

    /// <summary>Returns all module role assignments grouped by module (for bulk loads).</summary>
    Task<IList<ModuleRoleAssignment>> GetAllModuleRolesAsync(CancellationToken ct = default);

    /// <summary>Adds a role assignment to a module.</summary>
    Task AddModuleRoleAsync(ModuleRoleAssignment assignment, CancellationToken ct = default);

    /// <summary>Removes a role assignment from a module.</summary>
    void RemoveModuleRole(ModuleRoleAssignment assignment);

    /// <summary>Returns a specific module role assignment, or null.</summary>
    Task<ModuleRoleAssignment?> GetModuleRoleAsync(Guid moduleId, string roleName, CancellationToken ct = default);

    /// <summary>Commits all pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    // ── Sidebar Menu Items ────────────────────────────────────────────────
    /// <summary>Returns all top-level sidebar menu items (ParentId == null) with role accesses loaded.</summary>
    Task<IList<SidebarMenuItem>> GetTopLevelMenusAsync(CancellationToken ct = default);

    /// <summary>Returns sub-menu items for a given parent ID with role accesses loaded.</summary>
    Task<IList<SidebarMenuItem>> GetSubMenusAsync(Guid parentId, CancellationToken ct = default);

    /// <summary>Returns a sidebar menu item by ID with role accesses and sub-menus loaded.</summary>
    Task<SidebarMenuItem?> GetMenuByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns a sidebar menu item by stable key.</summary>
    Task<SidebarMenuItem?> GetMenuByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>Returns all menu items that are active and have the given role in their access list.</summary>
    Task<IList<SidebarMenuItem>> GetVisibleMenusForRoleAsync(string roleName, CancellationToken ct = default);

    /// <summary>Adds a new sidebar menu item.</summary>
    Task AddMenuAsync(SidebarMenuItem item, CancellationToken ct = default);

    /// <summary>Marks a sidebar menu item as modified.</summary>
    void UpdateMenu(SidebarMenuItem item);

    /// <summary>Adds a role access record.</summary>
    Task AddMenuRoleAccessAsync(SidebarMenuRoleAccess access, CancellationToken ct = default);

    /// <summary>Removes a role access record.</summary>
    void RemoveMenuRoleAccess(SidebarMenuRoleAccess access);

    /// <summary>Returns an existing role access record, or null.</summary>
    Task<SidebarMenuRoleAccess?> GetMenuRoleAccessAsync(Guid menuItemId, string roleName, CancellationToken ct = default);

    // ── Portal Settings ───────────────────────────────────────────────────
    /// <summary>Returns all portal branding settings as a dictionary (key → value).</summary>
    Task<Dictionary<string, string>> GetAllPortalSettingsAsync(CancellationToken ct = default);

    /// <summary>Returns a single portal setting value by key, or null if not set.</summary>
    Task<string?> GetPortalSettingAsync(string key, CancellationToken ct = default);

    /// <summary>Upserts a portal setting (creates if absent, updates value if present).</summary>
    Task UpsertPortalSettingAsync(string key, string value, CancellationToken ct = default);
}
