using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Represents an academic term (e.g. "Fall 2026", "Spring 2027").
/// Semesters are immutable once closed — grades, enrollments, and attendance records
/// linked to a closed semester cannot be altered via the UI or API.
/// </summary>
public class Semester : AuditableEntity
{
    /// <summary>Human-readable label shown throughout the portal (e.g. "Fall 2026").</summary>
    public string Name { get; private set; } = default!;

    /// <summary>UTC date the semester teaching period begins.</summary>
    public DateTime StartDate { get; private set; }

    /// <summary>UTC date the semester teaching period ends.</summary>
    public DateTime EndDate { get; private set; }

    /// <summary>
    /// When true the semester is locked — no new offerings, enrollments, or grade changes
    /// are permitted. Closing a semester is a one-way operation.
    /// </summary>
    public bool IsClosed { get; private set; }

    /// <summary>UTC timestamp recorded when the semester was closed by an Admin.</summary>
    public DateTime? ClosedAt { get; private set; }

    private Semester() { }

    public Semester(string name, DateTime startDate, DateTime endDate)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Permanently locks the semester.
    /// Throws InvalidOperationException if the semester is already closed — closing is irreversible.
    /// </summary>
    public void Close()
    {
        if (IsClosed)
            throw new InvalidOperationException($"Semester '{Name}' is already closed.");

        IsClosed = true;
        ClosedAt = DateTime.UtcNow;
        Touch();
    }
}
