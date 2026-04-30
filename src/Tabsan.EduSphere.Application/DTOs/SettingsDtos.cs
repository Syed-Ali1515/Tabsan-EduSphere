namespace Tabsan.EduSphere.Application.Dtos;

// ─────────────────────────────────────────────────────────────────────────────
// Report Settings DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Full report definition with its role assignments.</summary>
public record ReportDefinitionDto(
    Guid Id,
    string Key,
    string Name,
    string Purpose,
    bool IsActive,
    IList<string> AssignedRoles
);

/// <summary>Payload to create a new report definition.</summary>
public record CreateReportCommand(
    string Key,
    string Name,
    string Purpose
);

/// <summary>Payload to update an existing report definition.</summary>
public record UpdateReportCommand(
    string Name,
    string Purpose
);

/// <summary>Replaces all role assignments for a report or module (pass empty list to clear).</summary>
public record SetRolesCommand(IList<string> RoleNames);

// ─────────────────────────────────────────────────────────────────────────────
// Module Role Assignment DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Module with its currently assigned roles.</summary>
public record ModuleRolesDto(
    Guid ModuleId,
    string ModuleKey,
    string ModuleName,
    IList<string> AssignedRoles
);

// ─────────────────────────────────────────────────────────────────────────────
// Theme DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>The currently active theme for a user.</summary>
public record UserThemeDto(string? ThemeKey);

/// <summary>Payload to set the authenticated user's theme preference.</summary>
public record SetThemeCommand(string? ThemeKey);

// ─────────────────────────────────────────────────────────────────────────────
// Sidebar Menu Settings DTOs
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>A sidebar menu item with its role access list.</summary>
public record SidebarMenuItemDto(
    Guid     Id,
    string   Key,
    string   Name,
    string   Purpose,
    Guid?    ParentId,
    int      DisplayOrder,
    bool     IsActive,
    bool     IsSystemMenu,
    IList<SidebarMenuRoleAccessDto> RoleAccesses,
    IList<SidebarMenuItemDto>       SubMenus
);

/// <summary>Role access entry for a sidebar menu item.</summary>
public record SidebarMenuRoleAccessDto(
    string RoleName,
    bool   IsAllowed
);

/// <summary>Payload to replace all role access records for a menu item.</summary>
public record SetSidebarMenuRolesCommand(IList<SidebarRoleAccessEntry> Entries);

/// <summary>Single role + allowed flag pair used when updating sidebar role access.</summary>
public record SidebarRoleAccessEntry(string RoleName, bool IsAllowed);

/// <summary>Payload to toggle the active status of a sidebar menu item.</summary>
public record SetSidebarMenuStatusCommand(bool IsActive);
