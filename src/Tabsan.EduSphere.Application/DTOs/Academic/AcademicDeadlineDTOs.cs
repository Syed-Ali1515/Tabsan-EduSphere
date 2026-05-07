namespace Tabsan.EduSphere.Application.DTOs.Academic;

// ── Request DTOs ──────────────────────────────────────────────────────────────

/// <summary>Payload to create a new academic deadline.</summary>
public record CreateDeadlineRequest(
    Guid     SemesterId,
    string   Title,
    DateTime DeadlineDate,
    int      ReminderDaysBefore = 3,
    string?  Description = null);

/// <summary>Payload to update an existing academic deadline.</summary>
public record UpdateDeadlineRequest(
    string   Title,
    DateTime DeadlineDate,
    int      ReminderDaysBefore,
    string?  Description,
    bool     IsActive);

// ── Response DTOs ─────────────────────────────────────────────────────────────

/// <summary>Full deadline detail returned from create / get-by-id.</summary>
public record DeadlineResponse(
    Guid     Id,
    Guid     SemesterId,
    string   SemesterName,
    string   Title,
    string?  Description,
    DateTime DeadlineDate,
    int      ReminderDaysBefore,
    bool     IsActive,
    int      DaysUntilDeadline);

/// <summary>Slim summary used in list endpoints.</summary>
public record DeadlineSummary(
    Guid     Id,
    Guid     SemesterId,
    string   SemesterName,
    string   Title,
    DateTime DeadlineDate,
    int      DaysUntilDeadline,
    bool     IsActive);
