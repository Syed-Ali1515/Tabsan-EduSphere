using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository contract for AcademicDeadline operations.</summary>
public interface IAcademicDeadlineRepository
{
    /// <summary>Returns all active deadlines for a semester, ordered by DeadlineDate ascending.</summary>
    Task<IReadOnlyList<AcademicDeadline>> GetBySemesterAsync(Guid semesterId, CancellationToken ct = default);

    /// <summary>Returns all active deadlines across all semesters, ordered by DeadlineDate ascending.</summary>
    Task<IReadOnlyList<AcademicDeadline>> GetAllActiveAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns deadlines whose reminder has not yet been sent and whose
    /// (DeadlineDate - ReminderDaysBefore) is on or before today (UTC).
    /// </summary>
    Task<IReadOnlyList<AcademicDeadline>> GetPendingRemindersAsync(DateTime utcNow, CancellationToken ct = default);

    /// <summary>Returns the deadline with the given ID, or null.</summary>
    Task<AcademicDeadline?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Queues the deadline for insertion.</summary>
    Task AddAsync(AcademicDeadline deadline, CancellationToken ct = default);

    /// <summary>Marks the deadline as modified.</summary>
    void Update(AcademicDeadline deadline);

    /// <summary>Commits pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
