using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository interface for Semester operations.</summary>
public interface ISemesterRepository
{
    /// <summary>Returns all semesters ordered by start date descending.</summary>
    Task<IReadOnlyList<Semester>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns the semester with the given ID, or null.</summary>
    Task<Semester?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns the most recent open (not closed) semester, or null if all are closed.</summary>
    Task<Semester?> GetCurrentOpenAsync(CancellationToken ct = default);

    /// <summary>Queues the semester for insertion.</summary>
    Task AddAsync(Semester semester, CancellationToken ct = default);

    /// <summary>Marks the semester as modified.</summary>
    void Update(Semester semester);

    /// <summary>Commits changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
