using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Settings;

/// <summary>
/// Junction record that maps which roles can view a given report.
/// Super Admin always sees all reports regardless of this table.
/// </summary>
public class ReportRoleAssignment : BaseEntity
{
    /// <summary>FK to the report definition.</summary>
    public Guid ReportDefinitionId { get; private set; }

    /// <summary>Navigation to the report definition.</summary>
    public ReportDefinition ReportDefinition { get; private set; } = default!;

    /// <summary>
    /// Role name this assignment applies to (e.g. "Student", "Faculty", "Admin").
    /// Stored as string to avoid FK coupling to the Roles table.
    /// </summary>
    public string RoleName { get; private set; } = default!;

    private ReportRoleAssignment() { }

    public ReportRoleAssignment(Guid reportDefinitionId, string roleName)
    {
        ReportDefinitionId = reportDefinitionId;
        RoleName = roleName.Trim();
    }
}
