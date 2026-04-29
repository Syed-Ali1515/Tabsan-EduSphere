using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Settings;

/// <summary>
/// Defines a report available in the portal (e.g. "Attendance Summary", "CGPA Report").
/// Super Admin can activate/deactivate reports and assign which roles can see them.
/// Deactivated reports are hidden from all dashboards except Super Admin.
/// </summary>
public class ReportDefinition : AuditableEntity
{
    /// <summary>Display name shown in the report menu and settings table.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Short description of the report's purpose shown in System Settings.</summary>
    public string Purpose { get; private set; } = default!;

    /// <summary>
    /// Stable string key used to identify the report in code (e.g. "attendance_summary").
    /// Never changed after creation.
    /// </summary>
    public string Key { get; private set; } = default!;

    /// <summary>When false the report is hidden from all role dashboards except Super Admin.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Collection of role assignments for this report.</summary>
    public ICollection<ReportRoleAssignment> RoleAssignments { get; private set; } = new List<ReportRoleAssignment>();

    private ReportDefinition() { }

    public ReportDefinition(string key, string name, string purpose)
    {
        Key = key.Trim().ToLowerInvariant();
        Name = name.Trim();
        Purpose = purpose.Trim();
    }

    /// <summary>Activates the report so it becomes visible to all assigned roles.</summary>
    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    /// <summary>Deactivates the report — hidden from all roles except Super Admin. Data is preserved.</summary>
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    /// <summary>Updates the display name and purpose description.</summary>
    public void Update(string name, string purpose)
    {
        Name = name.Trim();
        Purpose = purpose.Trim();
        Touch();
    }
}
