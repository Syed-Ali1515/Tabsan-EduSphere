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

/// <summary>
/// Service contract for managing sidebar navigation visibility per role.
/// Super Admin always bypasses these settings and sees everything.
/// </summary>
public interface ISidebarMenuService
{
    /// <summary>Returns all top-level menu items with their role access list and sub-menus.</summary>
    Task<IList<SidebarMenuItemDto>> GetTopLevelMenusAsync(CancellationToken ct = default);

    /// <summary>Returns all sub-menu items under a given parent menu item.</summary>
    Task<IList<SidebarMenuItemDto>> GetSubMenusAsync(Guid parentId, CancellationToken ct = default);

    /// <summary>Returns a single menu item by ID with full detail (role accesses + sub-menus).</summary>
    Task<SidebarMenuItemDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns menu items visible to a given role (IsActive = true and role IsAllowed = true).</summary>
    Task<IList<SidebarMenuItemDto>> GetVisibleForRoleAsync(string roleName, CancellationToken ct = default);

    /// <summary>Replaces all role access entries for a menu item.</summary>
    Task SetRolesAsync(Guid id, SetSidebarMenuRolesCommand cmd, CancellationToken ct = default);

    /// <summary>Activates or deactivates a menu item. System menus cannot be deactivated.</summary>
    Task SetStatusAsync(Guid id, SetSidebarMenuStatusCommand cmd, CancellationToken ct = default);
}

/// <summary>
/// Service for reading and writing institution-wide portal branding settings.
/// Only SuperAdmin can save; any authenticated user can read.
/// </summary>
public interface IPortalBrandingService
{
    /// <summary>Returns the current branding values (defaults when not yet configured).</summary>
    Task<PortalBrandingDto> GetAsync(CancellationToken ct = default);

    /// <summary>Saves (upserts) all branding fields.</summary>
    Task SaveAsync(SavePortalBrandingCommand cmd, CancellationToken ct = default);
}

/// <summary>
/// Final-Touches Phase 30 Stage 30.2 — tenant onboarding, subscription, and profile settings service.
/// </summary>
public interface ITenantOperationsService
{
    Task<TenantOnboardingTemplateDto> GetOnboardingTemplateAsync(CancellationToken ct = default);
    Task SaveOnboardingTemplateAsync(SaveTenantOnboardingTemplateCommand cmd, CancellationToken ct = default);

    Task<TenantSubscriptionPlanDto> GetSubscriptionPlanAsync(CancellationToken ct = default);
    Task SaveSubscriptionPlanAsync(SaveTenantSubscriptionPlanCommand cmd, CancellationToken ct = default);

    Task<TenantProfileSettingsDto> GetTenantProfileAsync(CancellationToken ct = default);
    Task SaveTenantProfileAsync(SaveTenantProfileSettingsCommand cmd, CancellationToken ct = default);
}
