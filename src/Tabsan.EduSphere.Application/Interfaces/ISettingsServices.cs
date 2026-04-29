using Tabsan.EduSphere.Application.Dtos;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Service contract for managing system report definitions and their role assignments.
/// Only Super Admins can create/modify report definitions.
/// </summary>
public interface IReportSettingsService
{
    /// <summary>Returns all report definitions with their current role assignments.</summary>
    Task<IList<ReportDefinitionDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns a single report definition by its stable key.</summary>
    Task<ReportDefinitionDto> GetByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>Creates a new report definition.</summary>
    Task<ReportDefinitionDto> CreateAsync(CreateReportCommand cmd, CancellationToken ct = default);

    /// <summary>Updates the name and purpose of a report definition.</summary>
    Task<ReportDefinitionDto> UpdateAsync(Guid id, UpdateReportCommand cmd, CancellationToken ct = default);

    /// <summary>Activates a report so it appears on role dashboards.</summary>
    Task ActivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Deactivates a report so it's hidden from role dashboards (Super Admin still sees it).</summary>
    Task DeactivateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Replaces all role assignments for a report. Pass an empty list to clear all.</summary>
    Task SetRolesAsync(Guid id, SetRolesCommand cmd, CancellationToken ct = default);
}

/// <summary>
/// Service contract for managing per-module role assignments and per-user theme preferences.
/// </summary>
public interface IModuleRolesService
{
    /// <summary>Returns role assignments for a specific module by its key.</summary>
    Task<ModuleRolesDto> GetByModuleKeyAsync(string moduleKey, CancellationToken ct = default);

    /// <summary>Returns role assignments for all modules.</summary>
    Task<IList<ModuleRolesDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Replaces all role assignments for a module. Pass an empty list to clear all.</summary>
    Task SetRolesAsync(string moduleKey, SetRolesCommand cmd, CancellationToken ct = default);
}

/// <summary>Service contract for managing per-user theme preferences.</summary>
public interface IThemeService
{
    /// <summary>Returns the current authenticated user's theme key (null = system default).</summary>
    Task<UserThemeDto> GetThemeAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Sets (or clears) the theme key for the specified user.</summary>
    Task SetThemeAsync(Guid userId, SetThemeCommand cmd, CancellationToken ct = default);
}
