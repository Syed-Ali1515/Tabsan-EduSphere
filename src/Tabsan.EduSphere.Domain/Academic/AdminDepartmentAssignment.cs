using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Maps an Admin user account to one or more departments they are authorised to access.
/// SuperAdmin remains unrestricted and does not rely on this mapping.
/// </summary>
public class AdminDepartmentAssignment : BaseEntity
{
    /// <summary>FK to the Admin user account being assigned.</summary>
    public Guid AdminUserId { get; private set; }

    /// <summary>FK to the department the admin can access.</summary>
    public Guid DepartmentId { get; private set; }

    /// <summary>Navigation to the department.</summary>
    public Department Department { get; private set; } = default!;

    /// <summary>UTC timestamp when the assignment was created.</summary>
    public DateTime AssignedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when access was revoked. Null while active.</summary>
    public DateTime? RemovedAt { get; private set; }

    /// <summary>True while the assignment has not been revoked.</summary>
    public bool IsActive => RemovedAt == null;

    private AdminDepartmentAssignment() { }

    public AdminDepartmentAssignment(Guid adminUserId, Guid departmentId)
    {
        AdminUserId = adminUserId;
        DepartmentId = departmentId;
    }

    /// <summary>Revokes this admin's access to the department.</summary>
    public void Remove()
    {
        if (RemovedAt != null)
            throw new InvalidOperationException("This assignment has already been removed.");

        RemovedAt = DateTime.UtcNow;
    }
}
