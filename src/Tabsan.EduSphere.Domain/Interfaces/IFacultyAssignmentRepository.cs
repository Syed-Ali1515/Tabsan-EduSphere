using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository interface for FacultyDepartmentAssignment operations.</summary>
public interface IFacultyAssignmentRepository
{
    /// <summary>Returns all active department assignments for the given faculty user.</summary>
    Task<IReadOnlyList<FacultyDepartmentAssignment>> GetByFacultyAsync(Guid facultyUserId, CancellationToken ct = default);

    /// <summary>Returns all active faculty assignments for the given department.</summary>
    Task<IReadOnlyList<FacultyDepartmentAssignment>> GetByDepartmentAsync(Guid departmentId, CancellationToken ct = default);

    /// <summary>Returns the active assignment for the given faculty + department pair, or null.</summary>
    Task<FacultyDepartmentAssignment?> GetAsync(Guid facultyUserId, Guid departmentId, CancellationToken ct = default);

    /// <summary>Returns the list of department IDs the faculty user is currently assigned to.</summary>
    Task<IReadOnlyList<Guid>> GetDepartmentIdsForFacultyAsync(Guid facultyUserId, CancellationToken ct = default);

    /// <summary>Queues the assignment for insertion.</summary>
    Task AddAsync(FacultyDepartmentAssignment assignment, CancellationToken ct = default);

    /// <summary>Marks the assignment as modified.</summary>
    void Update(FacultyDepartmentAssignment assignment);

    /// <summary>Commits changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
