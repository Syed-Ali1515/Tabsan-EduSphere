using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

// Final-Touches Phase 17 Stage 17.3 — course type enum for degree audit elective/core classification
public enum CourseType
{
    Core     = 1,
    Elective = 2
}

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

    // Final-Touches Phase 17 Stage 17.3 — core vs elective classification
    /// <summary>Whether this course is a core requirement or an elective.</summary>
    public CourseType CourseType { get; private set; } = CourseType.Core;

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

    // Final-Touches Phase 17 Stage 17.3 — set core/elective classification
    /// <summary>Sets the course type (Core or Elective).</summary>
    public void SetCourseType(CourseType courseType)
    {
        CourseType = courseType;
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
