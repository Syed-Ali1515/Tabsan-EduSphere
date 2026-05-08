namespace Tabsan.EduSphere.Domain.Enums;

/// <summary>
/// Identifies the academic operational model of the institution.
/// University is the default and matches all pre-Phase-23 behaviour.
/// </summary>
public enum InstitutionType
{
    /// <summary>
    /// Semester-based degree programs with GPA / CGPA tracking.
    /// Default mode — all existing Phase 1–22 functionality targets this mode.
    /// </summary>
    University = 0,

    /// <summary>
    /// Grade/class-based system (Grades 1–12), streams for Grades 9–12,
    /// percentage-based results, and end-of-year promotion rules.
    /// </summary>
    School = 1,

    /// <summary>
    /// Year-based academic structure (Year 1, Year 2, …),
    /// percentage-based grading, and year-to-year progression.
    /// </summary>
    College = 2,
}
