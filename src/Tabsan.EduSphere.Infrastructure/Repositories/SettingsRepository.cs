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
}
