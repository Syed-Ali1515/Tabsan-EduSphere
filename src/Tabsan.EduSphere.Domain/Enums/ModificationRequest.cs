namespace Tabsan.EduSphere.Domain.Enums;

/// <summary>
/// Represents the modification type for teacher requests to modify attendance or results.
/// </summary>
public enum ModificationType
{
    /// <summary>Modification requested for an attendance record.</summary>
    Attendance = 1,

    /// <summary>Modification requested for a published result.</summary>
    Result = 2
}

/// <summary>
/// Represents the status of a teacher modification request.
/// Teachers can request modifications to attendance or results with a reason.
/// Admins review and approve/reject the request.
/// </summary>
public enum ModificationRequestStatus
{
    /// <summary>Request submitted, awaiting admin review.</summary>
    Pending = 1,

    /// <summary>Admin has approved the modification; the record has been updated.</summary>
    Approved = 2,

    /// <summary>Admin has rejected the modification; the record remains unchanged.</summary>
    Rejected = 3,

    /// <summary>Request was cancelled by the teacher before admin review.</summary>
    Cancelled = 4
}
