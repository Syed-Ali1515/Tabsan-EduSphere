using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Modules;
using Tabsan.EduSphere.Domain.Settings;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Manages report definitions and their role assignments on behalf of Super Admin.
/// All changes are persisted immediately — no caching needed for admin settings.
/// </summary>
public class ReportSettingsService : IReportSettingsService
{
    private readonly ISettingsRepository _repo;

    public ReportSettingsService(ISettingsRepository repo) => _repo = repo;

    public async Task<IList<ReportDefinitionDto>> GetAllAsync(CancellationToken ct = default)
    {
        var reports = await _repo.GetAllReportsAsync(ct);
        return reports.Select(MapDto).ToList();
    }

    public async Task<ReportDefinitionDto> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        var report = await _repo.GetReportByKeyAsync(key, ct)
            ?? throw new KeyNotFoundException($"Report '{key}' not found.");
        return MapDto(report);
    }

    public async Task<ReportDefinitionDto> CreateAsync(CreateReportCommand cmd, CancellationToken ct = default)
    {
        var existing = await _repo.GetReportByKeyAsync(cmd.Key, ct);
        if (existing is not null)
            throw new InvalidOperationException($"A report with key '{cmd.Key}' already exists.");

        var report = new ReportDefinition(cmd.Key, cmd.Name, cmd.Purpose);
        await _repo.AddReportAsync(report, ct);
        await _repo.SaveChangesAsync(ct);
        return MapDto(report);
    }

    public async Task<ReportDefinitionDto> UpdateAsync(Guid id, UpdateReportCommand cmd, CancellationToken ct = default)
    {
        var report = await _repo.GetReportByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Report {id} not found.");

        report.Update(cmd.Name, cmd.Purpose);
        _repo.UpdateReport(report);
        await _repo.SaveChangesAsync(ct);
        return MapDto(report);
    }

    public async Task ActivateAsync(Guid id, CancellationToken ct = default)
    {
        var report = await _repo.GetReportByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Report {id} not found.");

        report.Activate();
        _repo.UpdateReport(report);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
    {
        var report = await _repo.GetReportByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Report {id} not found.");

        report.Deactivate();
        _repo.UpdateReport(report);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task SetRolesAsync(Guid id, SetRolesCommand cmd, CancellationToken ct = default)
    {
        var report = await _repo.GetReportByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Report {id} not found.");

        var normalizedNew = cmd.RoleNames.Select(r => r.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existing = report.RoleAssignments.ToList();

        // Remove roles no longer in the new set
        foreach (var assignment in existing)
        {
            if (!normalizedNew.Contains(assignment.RoleName))
                _repo.RemoveReportRole(assignment);
        }

        // Add roles not yet assigned
        var existingNames = existing.Select(a => a.RoleName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var roleName in normalizedNew)
        {
            if (!existingNames.Contains(roleName))
                await _repo.AddReportRoleAsync(new ReportRoleAssignment(id, roleName), ct);
        }

        await _repo.SaveChangesAsync(ct);
    }

    private static ReportDefinitionDto MapDto(ReportDefinition r) => new(
        r.Id, r.Key, r.Name, r.Purpose, r.IsActive,
        r.RoleAssignments.Select(a => a.RoleName).ToList()
    );
}

/// <summary>
/// Manages per-module role assignments.
/// Super Admin can specify which roles can access each module
/// (in addition to the mandatory/license-based access).
/// </summary>
public class ModuleRolesService : IModuleRolesService
{
    private readonly ISettingsRepository _settingsRepo;
    private readonly IModuleRepository _moduleRepo;

    public ModuleRolesService(ISettingsRepository settingsRepo, IModuleRepository moduleRepo)
    {
        _settingsRepo = settingsRepo;
        _moduleRepo = moduleRepo;
    }

    public async Task<ModuleRolesDto> GetByModuleKeyAsync(string moduleKey, CancellationToken ct = default)
    {
        var status = await _moduleRepo.GetStatusByKeyAsync(moduleKey, ct)
            ?? throw new KeyNotFoundException($"Module '{moduleKey}' not found.");

        var assignments = await _settingsRepo.GetModuleRolesAsync(status.ModuleId, ct);
        return new ModuleRolesDto(
            status.ModuleId,
            status.Module.Key,
            status.Module.Name,
            assignments.Select(a => a.RoleName).ToList()
        );
    }

    public async Task<IList<ModuleRolesDto>> GetAllAsync(CancellationToken ct = default)
    {
        var allAssignments = await _settingsRepo.GetAllModuleRolesAsync(ct);
        var modules = await _moduleRepo.GetAllWithStatusAsync(ct);

        return modules.Select(m => new ModuleRolesDto(
            m.Id,
            m.Key,
            m.Name,
            allAssignments.Where(a => a.ModuleId == m.Id).Select(a => a.RoleName).ToList()
        )).ToList();
    }

    public async Task SetRolesAsync(string moduleKey, SetRolesCommand cmd, CancellationToken ct = default)
    {
        var status = await _moduleRepo.GetStatusByKeyAsync(moduleKey, ct)
            ?? throw new KeyNotFoundException($"Module '{moduleKey}' not found.");

        var moduleId = status.ModuleId;
        var normalizedNew = cmd.RoleNames.Select(r => r.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var existing = await _settingsRepo.GetModuleRolesAsync(moduleId, ct);

        // Remove roles no longer in the new set
        foreach (var assignment in existing)
        {
            if (!normalizedNew.Contains(assignment.RoleName))
                _settingsRepo.RemoveModuleRole(assignment);
        }

        // Add roles not yet assigned
        var existingNames = existing.Select(a => a.RoleName).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var roleName in normalizedNew)
        {
            if (!existingNames.Contains(roleName))
                await _settingsRepo.AddModuleRoleAsync(new ModuleRoleAssignment(moduleId, roleName), ct);
        }

        await _settingsRepo.SaveChangesAsync(ct);
    }
}

/// <summary>
/// Manages per-user UI theme preferences. Reads and writes the ThemeKey field on the User entity.
/// </summary>
public class ThemeService : IThemeService
{
    private readonly IUserRepository _userRepo;

    public ThemeService(IUserRepository userRepo) => _userRepo = userRepo;

    public async Task<UserThemeDto> GetThemeAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException($"User {userId} not found.");
        return new UserThemeDto(user.ThemeKey);
    }

    public async Task SetThemeAsync(Guid userId, SetThemeCommand cmd, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByIdAsync(userId, ct)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        user.SetTheme(cmd.ThemeKey);
        _userRepo.Update(user);
        await _userRepo.SaveChangesAsync(ct);
    }
}

// -----------------------------------------------------------------------------
// SidebarMenuService
// -----------------------------------------------------------------------------

/// <summary>
/// Manages sidebar navigation menu visibility per role.
/// Super Admin always bypasses these settings � the service exposes data only;
/// Super Admin enforcement is done in the sidebar rendering layer.
/// </summary>
public class SidebarMenuService : ISidebarMenuService
{
    private readonly ISettingsRepository _repo;

    public SidebarMenuService(ISettingsRepository repo) => _repo = repo;

    public async Task<IList<SidebarMenuItemDto>> GetTopLevelMenusAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetTopLevelMenusAsync(ct);
        return items.Select(Map).ToList();
    }

    public async Task<IList<SidebarMenuItemDto>> GetSubMenusAsync(Guid parentId, CancellationToken ct = default)
    {
        var items = await _repo.GetSubMenusAsync(parentId, ct);
        return items.Select(MapFlat).ToList();
    }

    public async Task<SidebarMenuItemDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var item = await _repo.GetMenuByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Sidebar menu item '{id}' not found.");
        return Map(item);
    }

    public async Task<IList<SidebarMenuItemDto>> GetVisibleForRoleAsync(string roleName, CancellationToken ct = default)
    {
        var items = await _repo.GetVisibleMenusForRoleAsync(roleName, ct);
        return items.Select(Map).ToList();
    }

    public async Task SetRolesAsync(Guid id, SetSidebarMenuRolesCommand cmd, CancellationToken ct = default)
    {
        var item = await _repo.GetMenuByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Sidebar menu item '{id}' not found.");

        var normalized = cmd.Entries
            .GroupBy(e => e.RoleName.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(g => g.Last())
            .ToDictionary(e => e.RoleName.Trim(), e => e.IsAllowed, StringComparer.OrdinalIgnoreCase);

        // Replace semantics: remove existing roles not present in payload.
        foreach (var existing in item.RoleAccesses.ToList())
        {
            if (!normalized.ContainsKey(existing.RoleName))
                _repo.RemoveMenuRoleAccess(existing);
        }

        foreach (var entry in normalized)
        {
            var existing = await _repo.GetMenuRoleAccessAsync(id, entry.Key, ct);
            if (existing is not null)
            {
                existing.SetAllowed(entry.Value);
            }
            else
            {
                var access = new SidebarMenuRoleAccess(id, entry.Key, entry.Value);
                await _repo.AddMenuRoleAccessAsync(access, ct);
            }
        }

        _repo.UpdateMenu(item);
        await _repo.SaveChangesAsync(ct);
    }

    public async Task SetStatusAsync(Guid id, SetSidebarMenuStatusCommand cmd, CancellationToken ct = default)
    {
        var item = await _repo.GetMenuByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"Sidebar menu item '{id}' not found.");

        if (cmd.IsActive)
            item.Activate();
        else
            item.Deactivate();   // throws for system menus

        _repo.UpdateMenu(item);
        await _repo.SaveChangesAsync(ct);
    }

    // -- Mapping helpers --------------------------------------------------

    private static SidebarMenuItemDto Map(SidebarMenuItem m) => new(
        m.Id,
        m.Key,
        m.Name,
        m.Purpose,
        m.ParentId,
        m.DisplayOrder,
        m.IsActive,
        m.IsSystemMenu,
        m.RoleAccesses.Select(r => new SidebarMenuRoleAccessDto(r.RoleName, r.IsAllowed)).ToList(),
        m.SubMenus.Select(MapFlat).ToList()
    );

    private static SidebarMenuItemDto MapFlat(SidebarMenuItem m) => new(
        m.Id,
        m.Key,
        m.Name,
        m.Purpose,
        m.ParentId,
        m.DisplayOrder,
        m.IsActive,
        m.IsSystemMenu,
        m.RoleAccesses.Select(r => new SidebarMenuRoleAccessDto(r.RoleName, r.IsAllowed)).ToList(),
        new List<SidebarMenuItemDto>()
    );
}

// -----------------------------------------------------------------------------
// PortalBrandingService
// -----------------------------------------------------------------------------

/// <summary>
/// Reads and writes institution branding values from the portal_settings key-value store.
/// </summary>
public class PortalBrandingService : IPortalBrandingService
{
    private const string KeyUniversityName  = "university_name";
    private const string KeyPortalSubtitle  = "portal_subtitle";
    private const string KeyFooterText      = "footer_text";
    private const string KeyLogoImage       = "logo_image";
    private const string KeyLegacyLogoUrl   = "logo_url";
    private const string KeyLegacyInitials  = "brand_initials";
    private const string KeyPrivacyPolicy   = "privacy_policy_url";
    private const string KeyPrivacyPolicyContent = "privacy_policy_content";
    private const string KeyFontFamily      = "font_family";
    private const string KeyFontSize        = "font_size";

    private readonly ISettingsRepository _repo;

    public PortalBrandingService(ISettingsRepository repo) => _repo = repo;

    public async Task<PortalBrandingDto> GetAsync(CancellationToken ct = default)
    {
        var all = await _repo.GetAllPortalSettingsAsync(ct);
        return new PortalBrandingDto(
            all.GetValueOrDefault(KeyUniversityName, "Tabsan EduSphere"),
            all.GetValueOrDefault(KeyPortalSubtitle, "Campus Portal"),
            all.GetValueOrDefault(KeyFooterText,     "© 2026 Tabsan EduSphere"),
            all.GetValueOrDefault(KeyLogoImage,      null),
            all.GetValueOrDefault(KeyPrivacyPolicy,  null),
            all.GetValueOrDefault(KeyPrivacyPolicyContent, null),
            all.GetValueOrDefault(KeyFontFamily,     null),
            all.GetValueOrDefault(KeyFontSize,       null)
        );
    }

    public async Task SaveAsync(SavePortalBrandingCommand cmd, CancellationToken ct = default)
    {
        await _repo.UpsertPortalSettingAsync(KeyUniversityName, cmd.UniversityName   ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyPortalSubtitle, cmd.PortalSubtitle   ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyFooterText,     cmd.FooterText       ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyLogoImage,      cmd.LogoImage        ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyPrivacyPolicy,  cmd.PrivacyPolicyUrl ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyPrivacyPolicyContent, cmd.PrivacyPolicyContent ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyFontFamily,     cmd.FontFamily       ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyFontSize,       cmd.FontSize         ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyLegacyInitials, string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyLegacyLogoUrl,  string.Empty, ct);
        await _repo.SaveChangesAsync(ct);
    }
}
