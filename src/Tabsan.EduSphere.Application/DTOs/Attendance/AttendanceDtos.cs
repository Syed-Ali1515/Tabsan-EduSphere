using Tabsan.EduSphere.Domain.Attendance;

namespace Tabsan.EduSphere.Application.DTOs.Attendance;

// ── Attendance DTOs ───────────────────────────────────────────────────────────

/// <summary>Request body for marking a single student's attendance for a session.</summary>
public sealed record MarkAttendanceRequest(
    Guid StudentProfileId,
    Guid CourseOfferingId,
    DateTime Date,
    AttendanceStatus Status,
    string? Remarks = null);

/// <summary>A single entry in a bulk attendance sheet (one row per student).</summary>
public sealed record BulkAttendanceEntry(
    Guid StudentProfileId,
    AttendanceStatus Status,
    string? Remarks = null);

/// <summary>Request body for bulk-marking a full class for one session.</summary>
public sealed record BulkMarkAttendanceRequest(
    Guid CourseOfferingId,
    DateTime Date,
    IReadOnlyList<BulkAttendanceEntry> Entries);

/// <summary>Request body for correcting an existing attendance record.</summary>
public sealed record CorrectAttendanceRequest(
    Guid StudentProfileId,
    Guid CourseOfferingId,
    DateTime Date,
    AttendanceStatus NewStatus,
    string? Remarks = null);

/// <summary>Read-model for a single attendance record.</summary>
public sealed record AttendanceResponse(
    Guid RecordId,
    Guid StudentProfileId,
    Guid CourseOfferingId,
    DateTime Date,
    string Status,
    string? Remarks,
    DateTime MarkedAt);

/// <summary>Attendance summary (percentage) for a student in an offering.</summary>
public sealed record AttendanceSummaryResponse(
    Guid StudentProfileId,
    Guid CourseOfferingId,
    int TotalSessions,
    int AttendedSessions,
    double AttendancePercent);
