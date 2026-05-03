using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Settings;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISettingsRepository.
/// Handles report definitions, report role assignments, and module role assignments.
/// </summary>
public class SettingsRepository : ISettingsRepository
{
    private readonly ApplicationDbContext _db;

    public SettingsRepository(ApplicationDbContext db) => _db = db;

    // ── Report Definitions ────────────────────────────────────────────────

    public Task<IList<ReportDefinition>> GetAllReportsAsync(CancellationToken ct = default)
        => _db.ReportDefinitions
              .Include(r => r.RoleAssignments)
              .OrderBy(r => r.Name)
              .ToListAsync(ct)
              .ContinueWith<IList<ReportDefinition>>(r => r.Result, ct);

    public Task<ReportDefinition?> GetReportByKeyAsync(string key, CancellationToken ct = default)
        => _db.ReportDefinitions
              .Include(r => r.RoleAssignments)
              .FirstOrDefaultAsync(r => r.Key == key.ToLowerInvariant(), ct);

    public Task<ReportDefinition?> GetReportByIdAsync(Guid id, CancellationToken ct = default)
        => _db.ReportDefinitions
              .Include(r => r.RoleAssignments)
              .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task AddReportAsync(ReportDefinition report, CancellationToken ct = default)
        => await _db.ReportDefinitions.AddAsync(report, ct);

    public void UpdateReport(ReportDefinition report) => _db.ReportDefinitions.Update(report);

    // ── Report Role Assignments ───────────────────────────────────────────

    public async Task AddReportRoleAsync(ReportRoleAssignment assignment, CancellationToken ct = default)
        => await _db.ReportRoleAssignments.AddAsync(assignment, ct);

    public void RemoveReportRole(ReportRoleAssignment assignment)
        => _db.ReportRoleAssignments.Remove(assignment);

    public Task<ReportRoleAssignment?> GetReportRoleAsync(Guid reportId, string roleName, CancellationToken ct = default)
        => _db.ReportRoleAssignments
              .FirstOrDefaultAsync(a => a.ReportDefinitionId == reportId
                                     && a.RoleName == roleName, ct);

    // ── Module Role Assignments ───────────────────────────────────────────

    public Task<IList<ModuleRoleAssignment>> GetModuleRolesAsync(Guid moduleId, CancellationToken ct = default)
        => _db.ModuleRoleAssignments
              .Where(a => a.ModuleId == moduleId)
              .ToListAsync(ct)
              .ContinueWith<IList<ModuleRoleAssignment>>(r => r.Result, ct);

    public Task<IList<ModuleRoleAssignment>> GetAllModuleRolesAsync(CancellationToken ct = default)
        => _db.ModuleRoleAssignments
              .Include(a => a.Module)
              .ToListAsync(ct)
              .ContinueWith<IList<ModuleRoleAssignment>>(r => r.Result, ct);

    public async Task AddModuleRoleAsync(ModuleRoleAssignment assignment, CancellationToken ct = default)
        => await _db.ModuleRoleAssignments.AddAsync(assignment, ct);

    public void RemoveModuleRole(ModuleRoleAssignment assignment)
        => _db.ModuleRoleAssignments.Remove(assignment);

    public Task<ModuleRoleAssignment?> GetModuleRoleAsync(Guid moduleId, string roleName, CancellationToken ct = default)
        => _db.ModuleRoleAssignments
              .FirstOrDefaultAsync(a => a.ModuleId == moduleId
                                      && a.RoleName == roleName, ct);

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);

    // ── Sidebar Menu Items ────────────────────────────────────────────────

    public Task<IList<SidebarMenuItem>> GetTopLevelMenusAsync(CancellationToken ct = default)
        => _db.SidebarMenuItems
              .Include(m => m.RoleAccesses)
              .Include(m => m.SubMenus).ThenInclude(s => s.RoleAccesses)
              .Where(m => m.ParentId == null)
              .OrderBy(m => m.DisplayOrder)
              .ToListAsync(ct)
              .ContinueWith<IList<SidebarMenuItem>>(r => r.Result, ct);

    public Task<IList<SidebarMenuItem>> GetSubMenusAsync(Guid parentId, CancellationToken ct = default)
        => _db.SidebarMenuItems
              .Include(m => m.RoleAccesses)
              .Where(m => m.ParentId == parentId)
              .OrderBy(m => m.DisplayOrder)
              .ToListAsync(ct)
              .ContinueWith<IList<SidebarMenuItem>>(r => r.Result, ct);

    public Task<SidebarMenuItem?> GetMenuByIdAsync(Guid id, CancellationToken ct = default)
        => _db.SidebarMenuItems
              .Include(m => m.RoleAccesses)
              .Include(m => m.SubMenus).ThenInclude(s => s.RoleAccesses)
              .FirstOrDefaultAsync(m => m.Id == id, ct);

    public Task<SidebarMenuItem?> GetMenuByKeyAsync(string key, CancellationToken ct = default)
        => _db.SidebarMenuItems
              .Include(m => m.RoleAccesses)
              .Include(m => m.SubMenus).ThenInclude(s => s.RoleAccesses)
              .FirstOrDefaultAsync(m => m.Key == key.ToLowerInvariant(), ct);

    public Task<IList<SidebarMenuItem>> GetVisibleMenusForRoleAsync(string roleName, CancellationToken ct = default)
        => _db.SidebarMenuItems
              .Include(m => m.RoleAccesses)
              .Include(m => m.SubMenus).ThenInclude(s => s.RoleAccesses)
              .Where(m => m.IsActive
                       && m.RoleAccesses.Any(r => r.RoleName == roleName && r.IsAllowed))
              .OrderBy(m => m.DisplayOrder)
              .ToListAsync(ct)
              .ContinueWith<IList<SidebarMenuItem>>(r => r.Result, ct);

    public async Task AddMenuAsync(SidebarMenuItem item, CancellationToken ct = default)
        => await _db.SidebarMenuItems.AddAsync(item, ct);

    public void UpdateMenu(SidebarMenuItem item) => _db.SidebarMenuItems.Update(item);

    public async Task AddMenuRoleAccessAsync(SidebarMenuRoleAccess access, CancellationToken ct = default)
        => await _db.SidebarMenuRoleAccesses.AddAsync(access, ct);

    public void RemoveMenuRoleAccess(SidebarMenuRoleAccess access)
        => _db.SidebarMenuRoleAccesses.Remove(access);

    public Task<SidebarMenuRoleAccess?> GetMenuRoleAccessAsync(Guid menuItemId, string roleName, CancellationToken ct = default)
        => _db.SidebarMenuRoleAccesses
              .FirstOrDefaultAsync(a => a.SidebarMenuItemId == menuItemId
                                     && a.RoleName == roleName, ct);

    // ── Portal Settings ────────────────────────────────────────────────────────

    public async Task<Dictionary<string, string>> GetAllPortalSettingsAsync(CancellationToken ct = default)
    {
        var rows = await _db.PortalSettings.ToListAsync(ct);
        return rows.ToDictionary(r => r.Key, r => r.Value, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<string?> GetPortalSettingAsync(string key, CancellationToken ct = default)
    {
        var row = await _db.PortalSettings.FirstOrDefaultAsync(p => p.Key == key.ToLowerInvariant(), ct);
        return row?.Value;
    }

    public async Task UpsertPortalSettingAsync(string key, string value, CancellationToken ct = default)
    {
        var normalizedKey = key.Trim().ToLowerInvariant();
        var existing = await _db.PortalSettings.FirstOrDefaultAsync(p => p.Key == normalizedKey, ct);
        if (existing is null)
        {
            await _db.PortalSettings.AddAsync(new PortalSetting(normalizedKey, value), ct);
        }
        else
        {
            existing.SetValue(value);
            _db.PortalSettings.Update(existing);
        }
    }
}
