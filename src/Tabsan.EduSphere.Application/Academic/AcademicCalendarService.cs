using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Application.Academic;

/// <summary>
/// Business logic for the Academic Calendar feature (Phase 12, Stages 12.1 and 12.2).
/// Manages named deadlines and dispatches reminder notifications.
/// </summary>
public class AcademicCalendarService : IAcademicCalendarService
{
    private readonly IAcademicDeadlineRepository _repo;
    private readonly INotificationService        _notifications;
    private readonly IUserRepository             _users;

    public AcademicCalendarService(
        IAcademicDeadlineRepository repo,
        INotificationService notifications,
        IUserRepository users)
    {
        _repo          = repo;
        _notifications = notifications;
        _users         = users;
    }

    public async Task<IReadOnlyList<DeadlineSummary>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllActiveAsync(ct);
        return items.Select(ToSummary).ToList();
    }

    public async Task<IReadOnlyList<DeadlineSummary>> GetBySemesterAsync(Guid semesterId, CancellationToken ct = default)
    {
        var items = await _repo.GetBySemesterAsync(semesterId, ct);
        return items.Select(ToSummary).ToList();
    }

    public async Task<DeadlineResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var d = await _repo.GetByIdAsync(id, ct);
        return d is null ? null : ToResponse(d);
    }

    public async Task<DeadlineResponse> CreateAsync(CreateDeadlineRequest request, CancellationToken ct = default)
    {
        var deadline = new AcademicDeadline(
            request.SemesterId,
            request.Title,
            request.DeadlineDate,
            request.ReminderDaysBefore,
            request.Description);

        await _repo.AddAsync(deadline, ct);
        await _repo.SaveChangesAsync(ct);

        // Reload with navigation so SemesterName is populated.
        var created = (await _repo.GetByIdAsync(deadline.Id, ct))!;
        return ToResponse(created);
    }

    public async Task<DeadlineResponse?> UpdateAsync(Guid id, UpdateDeadlineRequest request, CancellationToken ct = default)
    {
        var deadline = await _repo.GetByIdAsync(id, ct);
        if (deadline is null) return null;

        deadline.Update(request.Title, request.DeadlineDate, request.ReminderDaysBefore,
                        request.Description, request.IsActive);
        _repo.Update(deadline);
        await _repo.SaveChangesAsync(ct);
        return ToResponse(deadline);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var deadline = await _repo.GetByIdAsync(id, ct);
        if (deadline is null) return false;

        deadline.SoftDelete();
        _repo.Update(deadline);
        await _repo.SaveChangesAsync(ct);
        return true;
    }

    public async Task<int> DispatchPendingRemindersAsync(CancellationToken ct = default)
    {
        var pending = await _repo.GetPendingRemindersAsync(DateTime.UtcNow, ct);
        if (pending.Count == 0) return 0;

        // Notify all active students, faculty, and admins.
        var recipients = await _users.GetActiveUsersByRolesAsync(
            new[] { "Student", "Faculty", "Admin", "SuperAdmin" }, ct);
        var recipientIds = recipients.Select(u => u.Id).ToList();

        int dispatched = 0;
        foreach (var d in pending)
        {
            if (recipientIds.Count == 0) break;

            int days = (int)Math.Ceiling((d.DeadlineDate - DateTime.UtcNow).TotalDays);
            var body = days > 0
                ? $"Reminder: '{d.Title}' is due in {days} day(s) on {d.DeadlineDate:dd MMM yyyy}."
                : $"Reminder: '{d.Title}' is due today ({d.DeadlineDate:dd MMM yyyy}).";

            await _notifications.SendSystemAsync(
                title: $"Deadline Reminder: {d.Title}",
                body: body,
                type: NotificationType.System,
                recipientUserIds: recipientIds,
                ct: ct);

            d.MarkReminderSent();
            _repo.Update(d);
            dispatched++;
        }

        if (dispatched > 0)
            await _repo.SaveChangesAsync(ct);

        return dispatched;
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    private static DeadlineSummary ToSummary(AcademicDeadline d) => new(
        d.Id,
        d.SemesterId,
        d.Semester.Name,
        d.Title,
        d.DeadlineDate,
        DaysUntil(d.DeadlineDate),
        d.IsActive);

    private static DeadlineResponse ToResponse(AcademicDeadline d) => new(
        d.Id,
        d.SemesterId,
        d.Semester.Name,
        d.Title,
        d.Description,
        d.DeadlineDate,
        d.ReminderDaysBefore,
        d.IsActive,
        DaysUntil(d.DeadlineDate));

    private static int DaysUntil(DateTime deadlineDate)
        => (int)Math.Ceiling((deadlineDate - DateTime.UtcNow).TotalDays);
}
