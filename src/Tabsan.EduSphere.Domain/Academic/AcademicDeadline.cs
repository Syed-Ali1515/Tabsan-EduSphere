using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// A named academic deadline attached to a semester (e.g. census date, exam period, assignment cutoff).
/// Admins and SuperAdmins create and manage deadlines; all roles can view them on the calendar.
/// The background reminder job reads ReminderDaysBefore to dispatch notifications in advance.
/// </summary>
public class AcademicDeadline : AuditableEntity
{
    /// <summary>The semester this deadline belongs to.</summary>
    public Guid SemesterId { get; private set; }

    /// <summary>Navigation property to the parent semester.</summary>
    public Semester Semester { get; private set; } = default!;

    /// <summary>Short label shown on the calendar (e.g. "Census Date", "Exam Period End").</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Optional longer description displayed in the detail tooltip / list view.</summary>
    public string? Description { get; private set; }

    /// <summary>UTC date of the deadline.</summary>
    public DateTime DeadlineDate { get; private set; }

    /// <summary>
    /// How many days before DeadlineDate the reminder notification should be dispatched.
    /// 0 means no advance reminder (notification fires on the day).
    /// </summary>
    public int ReminderDaysBefore { get; private set; }

    /// <summary>When false, the deadline is hidden from calendars and no reminders are sent.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>UTC timestamp recorded when the reminder notification was last dispatched. Null if not yet sent.</summary>
    public DateTime? LastReminderSentAt { get; private set; }

#pragma warning disable CS8618
    private AcademicDeadline() { }
#pragma warning restore CS8618

    public AcademicDeadline(Guid semesterId, string title, DateTime deadlineDate,
        int reminderDaysBefore = 3, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (reminderDaysBefore < 0)
            throw new ArgumentOutOfRangeException(nameof(reminderDaysBefore), "Must be >= 0.");

        SemesterId         = semesterId;
        Title              = title.Trim();
        Description        = description?.Trim();
        DeadlineDate       = deadlineDate;
        ReminderDaysBefore = reminderDaysBefore;
    }

    /// <summary>Updates editable fields. Used by Admin update endpoint.</summary>
    public void Update(string title, DateTime deadlineDate, int reminderDaysBefore, string? description, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (reminderDaysBefore < 0)
            throw new ArgumentOutOfRangeException(nameof(reminderDaysBefore));

        Title              = title.Trim();
        Description        = description?.Trim();
        DeadlineDate       = deadlineDate;
        ReminderDaysBefore = reminderDaysBefore;
        IsActive           = isActive;
        Touch();
    }

    /// <summary>Records that the reminder notification was dispatched.</summary>
    public void MarkReminderSent()
    {
        LastReminderSentAt = DateTime.UtcNow;
        Touch();
    }
}
