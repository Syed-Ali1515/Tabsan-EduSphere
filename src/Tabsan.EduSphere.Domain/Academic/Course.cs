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

    // Final-Touches Phase 19 Stage 19.1 — semester-based flag and total semester count
    /// <summary>True for degree programmes with defined semesters; false for short-duration courses.</summary>
    public bool HasSemesters { get; private set; } = true;

    /// <summary>Number of semesters to auto-generate when HasSemesters is true.</summary>
    public int? TotalSemesters { get; private set; }

    // Final-Touches Phase 19 Stage 19.2 — non-semester duration fields
    /// <summary>Duration value for non-semester courses (e.g. 6 for "6 Weeks").</summary>
    public int? DurationValue { get; private set; }

    /// <summary>Duration unit for non-semester courses: "Weeks", "Months", or "Years".</summary>
    public string? DurationUnit { get; private set; }

    // Final-Touches Phase 19 Stage 19.2 — grading type
    /// <summary>How results are graded: "GPA", "Percentage", or "Grade".</summary>
    public string GradingType { get; private set; } = "GPA";

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

    // Final-Touches Phase 19 Stage 19.1 — configure as semester-based course
    /// <summary>Configures this as a semester-based course and records how many semesters to auto-generate.</summary>
    public void SetSemesterBased(int totalSemesters, string gradingType = "GPA")
    {
        HasSemesters = true;
        TotalSemesters = totalSemesters;
        DurationValue = null;
        DurationUnit = null;
        GradingType = gradingType;
        Touch();
    }

    // Final-Touches Phase 19 Stage 19.2 — configure as non-semester (short-duration) course
    /// <summary>Configures this as a non-semester course with an explicit duration.</summary>
    public void SetNonSemesterBased(int durationValue, string durationUnit, string gradingType)
    {
        HasSemesters = false;
        TotalSemesters = null;
        DurationValue = durationValue;
        DurationUnit = durationUnit;
        GradingType = gradingType;
        Touch();
    }

    // Final-Touches Phase 19 Stage 19.2 — update grading type independently
    /// <summary>Updates the grading type (GPA / Percentage / Grade).</summary>
    public void SetGradingType(string gradingType)
    {
        GradingType = gradingType;
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
