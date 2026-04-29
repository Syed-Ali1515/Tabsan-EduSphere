using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Defines a course in the academic catalogue (e.g. "Introduction to Databases", "CS-301").
/// A Course is the reusable definition; a CourseOffering ties it to a specific semester and faculty.
/// </summary>
public class Course : AuditableEntity
{
    /// <summary>Full name displayed to students and faculty (e.g. "Data Structures and Algorithms").</summary>
    public string Title { get; private set; } = default!;

    /// <summary>
    /// Department-scoped unique code (e.g. "CS-301").
    /// Used on transcripts and reports.
    /// </summary>
    public string Code { get; private set; } = default!;

    /// <summary>Number of credit hours this course carries toward a degree.</summary>
    public int CreditHours { get; private set; }

    /// <summary>FK to the department that owns this course definition.</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>Navigation to the owning department.</summary>
    public Department Department { get; private set; } = default!;

    /// <summary>Controls whether the course can be offered in new semesters.</summary>
    public bool IsActive { get; private set; } = true;

    private Course() { }

    public Course(string title, string code, int creditHours, Guid departmentId)
    {
        Title = title;
        Code = code.ToUpperInvariant();
        CreditHours = creditHours;
        DepartmentId = departmentId;
    }

    /// <summary>Updates the course title.</summary>
    public void UpdateTitle(string newTitle)
    {
        Title = newTitle;
        Touch();
    }

    /// <summary>Retires the course so it cannot be offered in new semesters.</summary>
    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    /// <summary>Reinstates the course for future offerings.</summary>
    public void Activate()
    {
        IsActive = true;
        Touch();
    }
}
