using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Business operations for the Academic Calendar feature (Phase 12).
/// Manages named deadlines attached to semesters.
/// </summary>
public interface IAcademicCalendarService
{
    /// <summary>Returns all active deadlines across all semesters, ordered by date ascending.</summary>
    Task<IReadOnlyList<DeadlineSummary>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns active deadlines for a specific semester.</summary>
    Task<IReadOnlyList<DeadlineSummary>> GetBySemesterAsync(Guid semesterId, CancellationToken ct = default);

    /// <summary>Returns the full detail of a single deadline, or null if not found.</summary>
    Task<DeadlineResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Creates a new deadline. Returns the created deadline detail.</summary>
    Task<DeadlineResponse> CreateAsync(CreateDeadlineRequest request, CancellationToken ct = default);

    /// <summary>Updates an existing deadline. Returns the updated deadline, or null if not found.</summary>
    Task<DeadlineResponse?> UpdateAsync(Guid id, UpdateDeadlineRequest request, CancellationToken ct = default);

    /// <summary>Soft-deletes a deadline. Returns false if not found.</summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Called by the background reminder job.
    /// Dispatches in-app notifications for all deadlines whose reminder window has arrived
    /// and whose reminder has not yet been sent. Returns the number of reminders dispatched.
    /// </summary>
    Task<int> DispatchPendingRemindersAsync(CancellationToken ct = default);
}
