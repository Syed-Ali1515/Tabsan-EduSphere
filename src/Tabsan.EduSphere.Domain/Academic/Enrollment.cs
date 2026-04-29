using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Allowed values for the Enrollment.Status property.
/// </summary>
public enum EnrollmentStatus
{
    /// <summary>Student is actively enrolled in this course offering.</summary>
    Active = 1,

    /// <summary>Student dropped the course before the withdrawal deadline.</summary>
    Dropped = 2,

    /// <summary>The course offering was cancelled after enrollment — student was not charged.</summary>
    Cancelled = 3
}

/// <summary>
/// Records a student's enrollment in a specific CourseOffering.
/// Enrollment rows are NEVER deleted — they form part of the permanent academic history.
/// A student may drop (status changes to Dropped) but the row is preserved for transcript purposes.
/// </summary>
public class Enrollment : BaseEntity
{
    /// <summary>FK to the student's profile.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>Navigation to the student profile.</summary>
    public StudentProfile StudentProfile { get; private set; } = default!;

    /// <summary>FK to the course offering the student enrolled in.</summary>
    public Guid CourseOfferingId { get; private set; }

    /// <summary>Navigation to the course offering.</summary>
    public CourseOffering CourseOffering { get; private set; } = default!;

    /// <summary>UTC timestamp of enrollment action.</summary>
    public DateTime EnrolledAt { get; private set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp of drop action. Null while the student is active.</summary>
    public DateTime? DroppedAt { get; private set; }

    /// <summary>Current lifecycle state of this enrollment.</summary>
    public EnrollmentStatus Status { get; private set; } = EnrollmentStatus.Active;

    private Enrollment() { }

    public Enrollment(Guid studentProfileId, Guid courseOfferingId)
    {
        StudentProfileId = studentProfileId;
        CourseOfferingId = courseOfferingId;
    }

    /// <summary>
    /// Marks the enrollment as dropped by the student.
    /// Throws InvalidOperationException when the enrollment is not currently Active.
    /// </summary>
    public void Drop()
    {
        if (Status != EnrollmentStatus.Active)
            throw new InvalidOperationException("Only active enrollments can be dropped.");

        Status = EnrollmentStatus.Dropped;
        DroppedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the enrollment as cancelled due to an offering cancellation.
    /// Called by the system when an Admin cancels a CourseOffering after enrolment.
    /// </summary>
    public void Cancel()
    {
        Status = EnrollmentStatus.Cancelled;
        DroppedAt = DateTime.UtcNow;
    }
}
