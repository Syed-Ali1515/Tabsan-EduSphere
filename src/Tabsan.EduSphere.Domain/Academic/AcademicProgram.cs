using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Represents a degree programme offered by a department (e.g. "BS Computer Science", "MBA").
/// A programme belongs to exactly one department and defines the curriculum structure
/// that courses and enrollments are organised under.
/// </summary>
public class AcademicProgram : AuditableEntity
{
    /// <summary>Full official name of the programme.</summary>
    public string Name { get; private set; } = default!;

    /// <summary>
    /// Short unique code used in registration numbers and reports (e.g. "BSCS", "MBA").
    /// Always stored in uppercase.
    /// </summary>
    public string Code { get; private set; } = default!;

    /// <summary>FK to the department that owns this programme.</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>Navigation to the owning department.</summary>
    public Department Department { get; private set; } = default!;

    /// <summary>Total semesters / years required to complete the programme (e.g. 8 for a 4-year degree).</summary>
    public int TotalSemesters { get; private set; }

    /// <summary>Whether the programme is open for new student enrolment.</summary>
    public bool IsActive { get; private set; } = true;

    private AcademicProgram() { }

    public AcademicProgram(string name, string code, Guid departmentId, int totalSemesters)
    {
        Name = name;
        Code = code.ToUpperInvariant();
        DepartmentId = departmentId;
        TotalSemesters = totalSemesters;
    }

    /// <summary>Updates the programme's display name.</summary>
    public void Rename(string newName)
    {
        Name = newName;
        Touch();
    }

    /// <summary>Closes the programme so no new students can be enrolled under it.</summary>
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    /// <summary>Re-opens the programme for new enrolments.</summary>
    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}
