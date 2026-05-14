using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
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
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan SidebarCacheTtl = TimeSpan.FromSeconds(20);
    private static int _sidebarCacheVersion;

    public SidebarMenuService(ISettingsRepository repo, IMemoryCache cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<IList<SidebarMenuItemDto>> GetTopLevelMenusAsync(CancellationToken ct = default)
    {
        var cacheKey = $"sidebar:top:{CurrentSidebarCacheVersion()}";
        if (_cache.TryGetValue(cacheKey, out IList<SidebarMenuItemDto>? cached) && cached is not null)
            return cached;

        var items = await _repo.GetTopLevelMenusAsync(ct);
        var response = items.Select(Map).ToList();
        _cache.Set(cacheKey, response, SidebarCacheTtl);
        return response;
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
        var normalizedRole = roleName.Trim().ToLowerInvariant();
        var cacheKey = $"sidebar:visible:{CurrentSidebarCacheVersion()}:{normalizedRole}";
        if (_cache.TryGetValue(cacheKey, out IList<SidebarMenuItemDto>? cached) && cached is not null)
            return cached;

        var items = await _repo.GetVisibleMenusForRoleAsync(roleName, ct);
        var response = items.Select(Map).ToList();
        _cache.Set(cacheKey, response, SidebarCacheTtl);
        return response;
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
        InvalidateSidebarCache();
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
        InvalidateSidebarCache();
    }

    private static void InvalidateSidebarCache() => Interlocked.Increment(ref _sidebarCacheVersion);
    private static int CurrentSidebarCacheVersion() => Volatile.Read(ref _sidebarCacheVersion);

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

// -----------------------------------------------------------------------------
// TenantOperationsService
// -----------------------------------------------------------------------------

// Final-Touches Phase 30 Stage 30.2 — Tenant and Subscription Operations service.
public class TenantOperationsService : ITenantOperationsService
{
    private const string KeyOnboardingTemplateName     = "tenant_onboarding_template_name";
    private const string KeyOnboardingInstitutionMode  = "tenant_onboarding_institution_mode";
    private const string KeyOnboardingAdminRole        = "tenant_onboarding_admin_role";
    private const string KeyOnboardingWelcomeMessage   = "tenant_onboarding_welcome_message";
    private const string KeyOnboardingStarterModules   = "tenant_onboarding_starter_modules_csv";

    private const string KeyPlanKey                = "tenant_subscription_plan_key";
    private const string KeyPlanName               = "tenant_subscription_plan_name";
    private const string KeyPlanMonthlyPrice       = "tenant_subscription_monthly_price";
    private const string KeyPlanMaxUsers           = "tenant_subscription_max_users";
    private const string KeyPlanEnableLms          = "tenant_subscription_enable_lms";
    private const string KeyPlanEnablePayments     = "tenant_subscription_enable_payments";
    private const string KeyPlanEnableIntegrations = "tenant_subscription_enable_integrations";
    private const string KeyPlanIsActive           = "tenant_subscription_is_active";

    private const string KeyTenantCode         = "tenant_profile_code";
    private const string KeyTenantDisplayName  = "tenant_profile_display_name";
    private const string KeyTenantSupportEmail = "tenant_profile_support_email";
    private const string KeyTenantSupportPhone = "tenant_profile_support_phone";
    private const string KeyTenantTimeZone     = "tenant_profile_time_zone";
    private const string KeyTenantLocale       = "tenant_profile_locale";
    private const string KeyTenantCurrency     = "tenant_profile_currency_code";
    private const string KeyTenantBrandTheme   = "tenant_profile_branding_theme";

    private const string CacheKeyOnboardingTemplate = "tenant_ops:onboarding_template";
    private const string CacheKeySubscriptionPlan = "tenant_ops:subscription_plan";
    private const string CacheKeyTenantProfile = "tenant_ops:tenant_profile";
    private static readonly TimeSpan TenantOpsCacheTtl = TimeSpan.FromMinutes(5);

    private readonly ISettingsRepository _repo;
    private readonly IDistributedCache _distributedCache;

    public TenantOperationsService(ISettingsRepository repo, IDistributedCache distributedCache)
    {
        _repo = repo;
        _distributedCache = distributedCache;
    }

    public async Task<TenantOnboardingTemplateDto> GetOnboardingTemplateAsync(CancellationToken ct = default)
    {
        return await GetOrSetCachedAsync(CacheKeyOnboardingTemplate, async () =>
        {
            var all = await _repo.GetAllPortalSettingsAsync(ct);
            return new TenantOnboardingTemplateDto(
                all.GetValueOrDefault(KeyOnboardingTemplateName, "Standard University"),
                all.GetValueOrDefault(KeyOnboardingInstitutionMode, "University"),
                all.GetValueOrDefault(KeyOnboardingAdminRole, "Admin"),
                all.GetValueOrDefault(KeyOnboardingWelcomeMessage, "Welcome to Tabsan EduSphere."),
                all.GetValueOrDefault(KeyOnboardingStarterModules, "dashboard,students,courses,results")
            );
        }, ct);
    }

    public async Task SaveOnboardingTemplateAsync(SaveTenantOnboardingTemplateCommand cmd, CancellationToken ct = default)
    {
        await _repo.UpsertPortalSettingAsync(KeyOnboardingTemplateName, cmd.TemplateName ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyOnboardingInstitutionMode, cmd.DefaultInstitutionMode ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyOnboardingAdminRole, cmd.DefaultAdminRole ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyOnboardingWelcomeMessage, cmd.WelcomeMessage ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyOnboardingStarterModules, cmd.StarterModulesCsv ?? string.Empty, ct);
        await _repo.SaveChangesAsync(ct);
        await _distributedCache.RemoveAsync(CacheKeyOnboardingTemplate, ct);
    }

    public async Task<TenantSubscriptionPlanDto> GetSubscriptionPlanAsync(CancellationToken ct = default)
    {
        return await GetOrSetCachedAsync(CacheKeySubscriptionPlan, async () =>
        {
            var all = await _repo.GetAllPortalSettingsAsync(ct);

            var monthlyPrice = decimal.TryParse(all.GetValueOrDefault(KeyPlanMonthlyPrice), out var parsedPrice)
                ? parsedPrice
                : 0m;

            var maxUsers = int.TryParse(all.GetValueOrDefault(KeyPlanMaxUsers), out var parsedUsers)
                ? parsedUsers
                : 1000;

            return new TenantSubscriptionPlanDto(
                all.GetValueOrDefault(KeyPlanKey, "standard"),
                all.GetValueOrDefault(KeyPlanName, "Standard"),
                monthlyPrice,
                maxUsers,
                ParseBool(all.GetValueOrDefault(KeyPlanEnableLms), true),
                ParseBool(all.GetValueOrDefault(KeyPlanEnablePayments), true),
                ParseBool(all.GetValueOrDefault(KeyPlanEnableIntegrations), true),
                ParseBool(all.GetValueOrDefault(KeyPlanIsActive), true)
            );
        }, ct);
    }

    public async Task SaveSubscriptionPlanAsync(SaveTenantSubscriptionPlanCommand cmd, CancellationToken ct = default)
    {
        await _repo.UpsertPortalSettingAsync(KeyPlanKey, cmd.PlanKey ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyPlanName, cmd.PlanName ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyPlanMonthlyPrice, cmd.MonthlyPrice.ToString(System.Globalization.CultureInfo.InvariantCulture), ct);
        await _repo.UpsertPortalSettingAsync(KeyPlanMaxUsers, Math.Max(1, cmd.MaxUsers).ToString(), ct);
        await _repo.UpsertPortalSettingAsync(KeyPlanEnableLms, cmd.EnableLms.ToString(), ct);
        await _repo.UpsertPortalSettingAsync(KeyPlanEnablePayments, cmd.EnablePayments.ToString(), ct);
        await _repo.UpsertPortalSettingAsync(KeyPlanEnableIntegrations, cmd.EnableIntegrations.ToString(), ct);
        await _repo.UpsertPortalSettingAsync(KeyPlanIsActive, cmd.IsActive.ToString(), ct);
        await _repo.SaveChangesAsync(ct);
        await _distributedCache.RemoveAsync(CacheKeySubscriptionPlan, ct);
    }

    public async Task<TenantProfileSettingsDto> GetTenantProfileAsync(CancellationToken ct = default)
    {
        return await GetOrSetCachedAsync(CacheKeyTenantProfile, async () =>
        {
            var all = await _repo.GetAllPortalSettingsAsync(ct);
            return new TenantProfileSettingsDto(
                all.GetValueOrDefault(KeyTenantCode, "default-tenant"),
                all.GetValueOrDefault(KeyTenantDisplayName, "Tabsan EduSphere"),
                all.GetValueOrDefault(KeyTenantSupportEmail, "support@tabsan-edusphere.com"),
                all.GetValueOrDefault(KeyTenantSupportPhone, "+1-000-000-0000"),
                all.GetValueOrDefault(KeyTenantTimeZone, "UTC"),
                all.GetValueOrDefault(KeyTenantLocale, "en-US"),
                all.GetValueOrDefault(KeyTenantCurrency, "USD"),
                all.GetValueOrDefault(KeyTenantBrandTheme, "default")
            );
        }, ct);
    }

    public async Task SaveTenantProfileAsync(SaveTenantProfileSettingsCommand cmd, CancellationToken ct = default)
    {
        await _repo.UpsertPortalSettingAsync(KeyTenantCode, cmd.TenantCode ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyTenantDisplayName, cmd.TenantDisplayName ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyTenantSupportEmail, cmd.SupportEmail ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyTenantSupportPhone, cmd.SupportPhone ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyTenantTimeZone, cmd.TimeZone ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyTenantLocale, cmd.Locale ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyTenantCurrency, cmd.CurrencyCode ?? string.Empty, ct);
        await _repo.UpsertPortalSettingAsync(KeyTenantBrandTheme, cmd.BrandingTheme ?? string.Empty, ct);
        await _repo.SaveChangesAsync(ct);
        await _distributedCache.RemoveAsync(CacheKeyTenantProfile, ct);
    }

    private async Task<T> GetOrSetCachedAsync<T>(string key, Func<Task<T>> factory, CancellationToken ct) where T : class
    {
        var cached = await _distributedCache.GetStringAsync(key, ct);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var cachedValue = JsonSerializer.Deserialize<T>(cached);
            if (cachedValue is not null)
            {
                return cachedValue;
            }
        }

        var value = await factory();
        await _distributedCache.SetStringAsync(
            key,
            JsonSerializer.Serialize(value),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TenantOpsCacheTtl
            },
            ct);

        return value;
    }

    private static bool ParseBool(string? value, bool defaultValue)
        => bool.TryParse(value, out var parsed) ? parsed : defaultValue;
}

// -----------------------------------------------------------------------------
// FeatureFlagService
// -----------------------------------------------------------------------------

// Final-Touches Phase 30 Stage 30.3 — safe rollout and rollback flag operations.
public class FeatureFlagService : IFeatureFlagService
{
    private const string Prefix = "feature_flag:";
    private const string MetaSuffixDescription = ":description";
    private const string MetaSuffixUpdatedAt = ":updated_at_utc";
    private const string RollbackReasonKey = "feature_flag:last_rollback_reason";
    private const string RollbackAtKey = "feature_flag:last_rollback_at_utc";

    private static readonly Dictionary<string, bool> DefaultFlags = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tenant-operations.write"] = true,
        ["integration-gateway.enabled"] = true,
        ["gateway-diagnostics.enabled"] = true
    };

    private readonly ISettingsRepository _repo;

    public FeatureFlagService(ISettingsRepository repo)
    {
        _repo = repo;
    }

    public async Task<IList<FeatureFlagDto>> GetAllAsync(CancellationToken ct = default)
    {
        var all = await _repo.GetAllPortalSettingsAsync(ct);
        var keysFromStore = all.Keys
            .Where(k => k.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase) && !k.EndsWith(MetaSuffixDescription, StringComparison.OrdinalIgnoreCase) && !k.EndsWith(MetaSuffixUpdatedAt, StringComparison.OrdinalIgnoreCase))
            .Select(k => k[Prefix.Length..])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var defaultKey in DefaultFlags.Keys)
            keysFromStore.Add(defaultKey);

        var result = new List<FeatureFlagDto>(keysFromStore.Count);
        foreach (var key in keysFromStore.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
        {
            result.Add(ToDto(key, all));
        }

        return result;
    }

    public async Task<FeatureFlagDto> GetAsync(string key, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Feature flag key is required.", nameof(key));

        var all = await _repo.GetAllPortalSettingsAsync(ct);
        return ToDto(key.Trim().ToLowerInvariant(), all);
    }

    public async Task SaveAsync(SaveFeatureFlagCommand command, CancellationToken ct = default)
    {
        var key = NormalizeKey(command.Key);
        var now = DateTime.UtcNow;

        await _repo.UpsertPortalSettingAsync(WithPrefix(key), command.IsEnabled.ToString(), ct);
        await _repo.UpsertPortalSettingAsync(WithPrefix(key) + MetaSuffixUpdatedAt, now.ToString("O"), ct);
        if (command.Description is not null)
            await _repo.UpsertPortalSettingAsync(WithPrefix(key) + MetaSuffixDescription, command.Description, ct);

        await _repo.SaveChangesAsync(ct);
    }

    public async Task RollbackAsync(RollbackFeatureFlagsCommand command, CancellationToken ct = default)
    {
        var keys = (command.Keys ?? [])
            .Where(k => !string.IsNullOrWhiteSpace(k))
            .Select(NormalizeKey)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (keys.Count == 0)
            throw new InvalidOperationException("At least one feature flag key is required for rollback.");

        var now = DateTime.UtcNow;
        foreach (var key in keys)
        {
            await _repo.UpsertPortalSettingAsync(WithPrefix(key), bool.FalseString, ct);
            await _repo.UpsertPortalSettingAsync(WithPrefix(key) + MetaSuffixUpdatedAt, now.ToString("O"), ct);
        }

        await _repo.UpsertPortalSettingAsync(RollbackAtKey, now.ToString("O"), ct);
        if (!string.IsNullOrWhiteSpace(command.Reason))
            await _repo.UpsertPortalSettingAsync(RollbackReasonKey, command.Reason, ct);

        await _repo.SaveChangesAsync(ct);
    }

    private static FeatureFlagDto ToDto(string key, IReadOnlyDictionary<string, string> all)
    {
        var normalized = NormalizeKey(key);
        var rawValue = all.GetValueOrDefault(WithPrefix(normalized));
        var isEnabled = bool.TryParse(rawValue, out var parsed)
            ? parsed
            : DefaultFlags.GetValueOrDefault(normalized, false);

        var rawDescription = all.GetValueOrDefault(WithPrefix(normalized) + MetaSuffixDescription);
        var rawUpdatedAt = all.GetValueOrDefault(WithPrefix(normalized) + MetaSuffixUpdatedAt);
        var updatedAt = DateTime.TryParse(rawUpdatedAt, out var parsedTime)
            ? DateTime.SpecifyKind(parsedTime, DateTimeKind.Utc)
            : DateTime.UtcNow;

        return new FeatureFlagDto(
            normalized,
            isEnabled,
            string.IsNullOrWhiteSpace(rawDescription) ? null : rawDescription,
            updatedAt);
    }

    private static string WithPrefix(string key) => Prefix + key;

    private static string NormalizeKey(string key)
    {
        var normalized = key.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(normalized))
            throw new ArgumentException("Feature flag key is required.", nameof(key));

        return normalized;
    }
}
