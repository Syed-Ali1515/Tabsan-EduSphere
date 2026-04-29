using Tabsan.EduSphere.Domain.Common;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Identity;

namespace Tabsan.EduSphere.Domain.StudentLifecycle;

/// <summary>
/// Represents a teacher's request to modify an attendance record or published result.
/// Includes a mandatory reason field. Admins review and approve/reject the request.
/// On approval, the record is updated; on rejection, the record remains unchanged.
/// </summary>
public class TeacherModificationRequest : AuditableEntity
{
    /// <summary>FK to the teacher requesting the modification.</summary>
    public Guid TeacherUserId { get; private set; }

    /// <summary>Navigation to the teacher who made the request.</summary>
    public User Teacher { get; private set; } = default!;

    /// <summary>FK to the admin who approved/rejected the request. Null if still Pending.</summary>
    public Guid? ReviewedByUserId { get; private set; }

    /// <summary>Navigation to the admin who reviewed the request.</summary>
    public User? ReviewedByUser { get; private set; }

    /// <summary>Type of modification: Attendance or Result.</summary>
    public ModificationType ModificationType { get; private set; }

    /// <summary>
    /// ID of the record being modified. 
    /// For Attendance: AttendanceRecord ID.
    /// For Result: Result ID.
    /// </summary>
    public Guid RecordId { get; private set; }

    /// <summary>Current status: Pending, Approved, Rejected, or Cancelled.</summary>
    public ModificationRequestStatus Status { get; private set; } = ModificationRequestStatus.Pending;

    /// <summary>Mandatory reason provided by the teacher explaining why the modification is needed.</summary>
    public string Reason { get; private set; } = default!;

    /// <summary>
    /// JSON serialization of the proposed new values.
    /// For Attendance: { "status": "Present" }
    /// For Result: { "marks": 85 }
    /// </summary>
    public string ProposedData { get; private set; } = default!;

    /// <summary>Admin's comments when approving or rejecting the request.</summary>
    public string? AdminNotes { get; private set; }

    /// <summary>UTC timestamp when the request was reviewed. Null if still Pending.</summary>
    public DateTime? ReviewedAt { get; private set; }

    private TeacherModificationRequest() { }

    /// <summary>Creates a new teacher modification request.</summary>
    public TeacherModificationRequest(
        Guid teacherUserId,
        ModificationType modificationType,
        Guid recordId,
        string reason,
        string proposedData)
    {
        TeacherUserId = teacherUserId;
        ModificationType = modificationType;
        RecordId = recordId;
        Reason = reason;
        ProposedData = proposedData;
        Status = ModificationRequestStatus.Pending;
    }

    /// <summary>Admin approves the request. Update logic is handled by the service layer.</summary>
    public void Approve(Guid reviewedByUserId, string? notes = null)
    {
        if (Status != ModificationRequestStatus.Pending)
            throw new InvalidOperationException("Only Pending requests can be approved.");

        Status = ModificationRequestStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        AdminNotes = notes;
        ReviewedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Admin rejects the request. No changes are applied.</summary>
    public void Reject(Guid reviewedByUserId, string? notes = null)
    {
        if (Status != ModificationRequestStatus.Pending)
            throw new InvalidOperationException("Only Pending requests can be rejected.");

        Status = ModificationRequestStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        AdminNotes = notes;
        ReviewedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Teacher cancels their own request (before admin review).</summary>
    public void Cancel()
    {
        if (Status != ModificationRequestStatus.Pending)
            throw new InvalidOperationException("Only Pending requests can be cancelled.");

        Status = ModificationRequestStatus.Cancelled;
        Touch();
    }
}
