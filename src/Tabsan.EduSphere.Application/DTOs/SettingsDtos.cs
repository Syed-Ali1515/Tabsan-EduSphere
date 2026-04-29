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
