using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// A published timetable for a degree programme within a semester.
/// The display title is auto-derived: "Timetable for 5th Semester of BSCS. Effective 01-Sep-2025".
/// Admins create and populate timetables; publishing makes them visible to students and faculty.
/// Timetable data is downloadable as PDF or Excel.
/// </summary>
public class Timetable : AuditableEntity
{
    /// <summary>FK to the department this timetable belongs to.</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>Navigation to the department.</summary>
    public Department Department { get; private set; } = default!;

    /// <summary>FK to the semester this timetable covers.</summary>
    public Guid SemesterId { get; private set; }

    /// <summary>Navigation to the semester.</summary>
    public Semester Semester { get; private set; } = default!;

    /// <summary>FK to the degree programme this timetable belongs to (e.g. BSCS).</summary>
    public Guid AcademicProgramId { get; private set; }

    /// <summary>Navigation to the academic programme.</summary>
    public AcademicProgram AcademicProgram { get; private set; } = default!;

    /// <summary>Semester number within the programme (e.g. 5 for "5th Semester").</summary>
    public int SemesterNumber { get; private set; }

    /// <summary>Date from which this timetable is effective — used in the auto-generated title.</summary>
    public DateTime EffectiveDate { get; private set; }

    /// <summary>
    /// When true the timetable is visible to all users in the department.
    /// Unpublished timetables are only visible to Admin/SuperAdmin.
    /// </summary>
    public bool IsPublished { get; private set; }

    /// <summary>UTC timestamp when the timetable was published.</summary>
    public DateTime? PublishedAt { get; private set; }

    /// <summary>Collection of scheduled slots for this timetable.</summary>
    public ICollection<TimetableEntry> Entries { get; private set; } = new List<TimetableEntry>();

    private Timetable() { }

    public Timetable(Guid departmentId, Guid academicProgramId, Guid semesterId, int semesterNumber, DateTime effectiveDate)
    {
        DepartmentId = departmentId;
        AcademicProgramId = academicProgramId;
        SemesterId = semesterId;
        SemesterNumber = semesterNumber;
        EffectiveDate = effectiveDate.Date;
    }

    /// <summary>Updates the effective date and semester number.</summary>
    public void UpdateSchedule(int semesterNumber, DateTime effectiveDate)
    {
        SemesterNumber = semesterNumber;
        EffectiveDate = effectiveDate.Date;
        Touch();
    }

    /// <summary>
    /// Returns the auto-generated display title.
    /// Requires AcademicProgram navigation property to be loaded.
    /// </summary>
    public string GetTitle() =>
        $"Timetable for {Ordinal(SemesterNumber)} Semester of {AcademicProgram?.Code ?? "?"}. Effective {EffectiveDate:dd-MMM-yyyy}";

    private static string Ordinal(int n) => n switch
    {
        1 => "1st", 2 => "2nd", 3 => "3rd", _ => $"{n}th"
    };

    /// <summary>Publishes the timetable, making it visible to all department members.</summary>
    public void Publish()
    {
        if (IsPublished)
            throw new InvalidOperationException("Timetable is already published.");
        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Unpublishes the timetable (returns it to draft mode for editing).</summary>
    public void Unpublish()
    {
        IsPublished = false;
        PublishedAt = null;
        Touch();
    }
}
