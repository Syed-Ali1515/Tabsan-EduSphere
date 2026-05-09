using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

// Phase 26 — School and College Functional Expansion — Stage 26.1

/// <summary>
/// Records the assignment of a <see cref="SchoolStream"/> to a student.
/// A student may only be in one stream at a time; reassigning removes the previous link.
/// </summary>
public class StudentStreamAssignment : BaseEntity
{
    /// <summary>FK to the student profile receiving the stream assignment.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>FK to the assigned school stream.</summary>
    public Guid SchoolStreamId { get; private set; }

    /// <summary>Navigation to the assigned stream.</summary>
    public SchoolStream Stream { get; private set; } = default!;

    /// <summary>UTC timestamp when the stream was assigned.</summary>
    public DateTime AssignedAt { get; private set; }

    /// <summary>User ID of the admin or faculty member who created the assignment.</summary>
    public Guid AssignedByUserId { get; private set; }

    private StudentStreamAssignment() { }

    public StudentStreamAssignment(Guid studentProfileId, Guid schoolStreamId, Guid assignedByUserId)
    {
        StudentProfileId = studentProfileId;
        SchoolStreamId = schoolStreamId;
        AssignedByUserId = assignedByUserId;
        AssignedAt = DateTime.UtcNow;
    }
}
