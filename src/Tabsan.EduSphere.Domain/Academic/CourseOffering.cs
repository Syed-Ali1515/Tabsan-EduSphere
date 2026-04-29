using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Schedules a specific Course for a specific Semester, taught by a specific Faculty member.
/// Students enrol in CourseOfferings, not directly in Courses.
/// Once the parent Semester is closed this record becomes read-only.
/// </summary>
public class CourseOffering : AuditableEntity
{
    /// <summary>FK to the course catalogue entry being offered.</summary>
    public Guid CourseId { get; private set; }

    /// <summary>Navigation to the course definition.</summary>
    public Course Course { get; private set; } = default!;

    /// <summary>FK to the semester this offering is scheduled in.</summary>
    public Guid SemesterId { get; private set; }

    /// <summary>Navigation to the parent semester.</summary>
    public Semester Semester { get; private set; } = default!;

    /// <summary>
    /// FK to the Faculty User account responsible for teaching this offering.
    /// Nullable — an offering may be created before a faculty member is assigned.
    /// </summary>
    public Guid? FacultyUserId { get; private set; }

    /// <summary>Maximum number of students that can enrol in this section.</summary>
    public int MaxEnrollment { get; private set; }

    /// <summary>Controls whether students can enrol. Set to false to freeze new registrations.</summary>
    public bool IsOpen { get; private set; } = true;

    private CourseOffering() { }

    public CourseOffering(Guid courseId, Guid semesterId, int maxEnrollment, Guid? facultyUserId = null)
    {
        CourseId = courseId;
        SemesterId = semesterId;
        MaxEnrollment = maxEnrollment;
        FacultyUserId = facultyUserId;
    }

    /// <summary>Assigns or reassigns the teaching faculty for this offering.</summary>
    public void AssignFaculty(Guid facultyUserId)
    {
        FacultyUserId = facultyUserId;
        Touch();
    }

    /// <summary>Stops accepting new student enrolments without closing the offering entirely.</summary>
    public void Close()
    {
        IsOpen = false;
        Touch();
    }

    /// <summary>Re-opens enrolment for this offering (e.g. after a seat limit increase).</summary>
    public void Reopen()
    {
        IsOpen = true;
        Touch();
    }

    /// <summary>Changes the maximum seat count for this offering.</summary>
    public void UpdateMaxEnrollment(int newMax)
    {
        if (newMax < 1)
            throw new ArgumentOutOfRangeException(nameof(newMax), "Max enrollment must be at least 1.");
        MaxEnrollment = newMax;
        Touch();
    }
}
