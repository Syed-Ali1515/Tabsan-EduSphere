using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Attendance;

/// <summary>
/// The attendance status options for a single session.
/// Stored as a string in the database for readability.
/// </summary>
public enum AttendanceStatus
{
    /// <summary>Student was present for the session.</summary>
    Present = 1,

    /// <summary>Student was absent without an accepted excuse.</summary>
    Absent = 2,

    /// <summary>Student arrived late (within institution-defined grace period).</summary>
    Late = 3,

    /// <summary>Absence was officially excused (e.g., medical certificate accepted).</summary>
    Excused = 4
}

/// <summary>
/// Records the attendance of a single student for a single session of a <see cref="CourseOffering"/>.
/// Business rules:
///   - At most one record exists per (StudentProfileId, CourseOfferingId, Date).
/// Attendance records are NOT soft-deleted — corrections are done via <see cref="Correct"/>.
/// </summary>
public class AttendanceRecord : BaseEntity
{
    /// <summary>FK to the student whose attendance is being recorded.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>FK to the course offering session.</summary>
    public Guid CourseOfferingId { get; private set; }

    /// <summary>UTC date of the session (time component is ignored; use Date only).</summary>
    public DateTime Date { get; private set; }

    /// <summary>Attendance status for this session.</summary>
    public AttendanceStatus Status { get; private set; }

    /// <summary>FK to the faculty or admin user who recorded or last corrected this entry.</summary>
    public Guid MarkedByUserId { get; private set; }

    /// <summary>Optional faculty remark (e.g., "arrived 15 minutes late").</summary>
    public string? Remarks { get; private set; }

    // ── EF constructor ────────────────────────────────────────────────────────
    private AttendanceRecord() { }

    /// <summary>
    /// Records attendance for a student for a specific session date.
    /// <paramref name="date"/> is normalised to UTC date (time stripped).
    /// </summary>
    public AttendanceRecord(
        Guid studentProfileId,
        Guid courseOfferingId,
        DateTime date,
        AttendanceStatus status,
        Guid markedByUserId,
        string? remarks = null)
    {
        StudentProfileId = studentProfileId;
        CourseOfferingId = courseOfferingId;
        Date             = date.Date; // strip time component
        Status           = status;
        MarkedByUserId   = markedByUserId;
        Remarks          = remarks?.Trim();
        Touch();
    }

    // ── Behaviour ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Corrects the attendance status and optional remark after initial entry.
    /// Records the correcting user for audit purposes.
    /// </summary>
    public void Correct(AttendanceStatus newStatus, Guid correctedByUserId, string? remarks = null)
    {
        Status           = newStatus;
        MarkedByUserId   = correctedByUserId;
        Remarks          = remarks?.Trim();
        Touch();
    }
}
