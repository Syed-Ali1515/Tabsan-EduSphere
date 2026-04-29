using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Maps a Faculty user account to one or more departments they are authorised to access.
/// All faculty data queries (courses, students, attendance, results) are filtered
/// by the departments present in this table for the requesting faculty user.
/// </summary>
public class FacultyDepartmentAssignment : BaseEntity
{
    /// <summary>FK to the Faculty User account being assigned.</summary>
    public Guid FacultyUserId { get; private set; }

    /// <summary>FK to the department the faculty member is being given access to.</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>Navigation to the department.</summary>
    public Department Department { get; private set; } = default!;

    /// <summary>UTC timestamp when the assignment was created.</summary>
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when access was revoked. Null while the assignment is active.</summary>
    public DateTime? RemovedAt { get; private set; }

    /// <summary>Computed convenience property — true while the assignment has not been revoked.</summary>
    public bool IsActive => RemovedAt == null;

    private FacultyDepartmentAssignment() { }

    public FacultyDepartmentAssignment(Guid facultyUserId, Guid departmentId)
    {
        FacultyUserId = facultyUserId;
        DepartmentId = departmentId;
    }

    /// <summary>
    /// Revokes the faculty member's access to this department.
    /// A timestamp is recorded for audit purposes; the row is never deleted.
    /// </summary>
    public void Remove()
    {
        if (RemovedAt != null)
            throw new InvalidOperationException("This assignment has already been removed.");
        RemovedAt = DateTime.UtcNow;
    }
}
