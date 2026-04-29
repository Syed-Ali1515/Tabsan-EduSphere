using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository interface for StudentProfile operations.</summary>
public interface IStudentProfileRepository
{
    /// <summary>Returns the student profile linked to the given User ID, or null.</summary>
    Task<StudentProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Returns the student profile with the given profile ID, or null.</summary>
    Task<StudentProfile?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the student profile with the given registration number, or null.</summary>
    Task<StudentProfile?> GetByRegistrationNumberAsync(string registrationNumber, CancellationToken ct = default);

    /// <summary>Returns all student profiles, optionally filtered by department.</summary>
    Task<IReadOnlyList<StudentProfile>> GetAllAsync(Guid? departmentId = null, CancellationToken ct = default);

    /// <summary>Returns true when the registration number is already in use.</summary>
    Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct = default);

    /// <summary>Queues the profile for insertion.</summary>
    Task AddAsync(StudentProfile profile, CancellationToken ct = default);

    /// <summary>Marks the profile as modified.</summary>
    void Update(StudentProfile profile);

    /// <summary>Commits changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
