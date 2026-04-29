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
}
