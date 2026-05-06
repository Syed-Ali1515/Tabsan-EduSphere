using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository interface for AdminDepartmentAssignment operations.</summary>
public interface IAdminAssignmentRepository
{
    /// <summary>Returns all active department assignments for the given admin user.</summary>
    Task<IReadOnlyList<AdminDepartmentAssignment>> GetByAdminAsync(Guid adminUserId, CancellationToken ct = default);

    /// <summary>Returns all active admin assignments for the given department.</summary>
    Task<IReadOnlyList<AdminDepartmentAssignment>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);

    /// <summary>Returns the active assignment for the given admin + department pair, or null.</summary>
    Task<AdminDepartmentAssignment?> GetAsync(Guid adminUserId, Guid departmentId, CancellationToken ct = default);

    /// <summary>Returns the list of department IDs the admin user is currently assigned to.</summary>
    Task<IReadOnlyList<Guid>> GetDepartmentIdsForAdminAsync(Guid adminUserId, CancellationToken ct = default);

    /// <summary>Queues the assignment for insertion.</summary>
    Task AddAsync(AdminDepartmentAssignment assignment, CancellationToken ct = default);

    /// <summary>Marks the assignment as modified.</summary>
    void Update(AdminDepartmentAssignment assignment);

    /// <summary>Commits changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
