using Tabsan.EduSphere.Domain.Common;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Identity;

namespace Tabsan.EduSphere.Domain.StudentLifecycle;

/// <summary>
/// Represents a student or teacher's request to modify their profile information.
/// Examples: change name, address, phone number, email, etc.
/// Admins must review and approve/reject the change before it is applied.
/// </summary>
public class AdminChangeRequest : AuditableEntity
{
    /// <summary>FK to the user requesting the change.</summary>
    public Guid RequestorUserId { get; private set; }

    /// <summary>Navigation to the user who made the request.</summary>
    public User Requestor { get; private set; } = default!;

    /// <summary>FK to the admin who approved/rejected the request. Null if still Pending.</summary>
    public Guid? ReviewedByUserId { get; private set; }

    /// <summary>Navigation to the admin who reviewed the request.</summary>
    public User? ReviewedByUser { get; private set; }

    /// <summary>Current status of the request: Pending, Approved, Rejected, or Cancelled.</summary>
    public ChangeRequestStatus Status { get; private set; } = ChangeRequestStatus.Pending;

    /// <summary>Short description of what the student/teacher is requesting to change (e.g., "Update name from X to Y").</summary>
    public string ChangeDescription { get; private set; } = default!;

    /// <summary>Optional detailed reason or justification provided by the requestor.</summary>
    public string? Reason { get; private set; }

    /// <summary>
    /// JSON serialization of the new data. Structure depends on ChangeDescription.
    /// Example: { "name": "New Name", "email": "new@email.com" }
    /// </summary>
    public string NewData { get; private set; } = default!;

    /// <summary>Admin's comments when approving or rejecting the request.</summary>
    public string? AdminNotes { get; private set; }

    /// <summary>UTC timestamp when the request was reviewed (approved/rejected). Null if still Pending.</summary>
    public DateTime? ReviewedAt { get; private set; }

    private AdminChangeRequest() { }

    /// <summary>Creates a new admin change request.</summary>
    public AdminChangeRequest(Guid requestorUserId, string changeDescription, string newData, string? reason = null)
    {
        RequestorUserId = requestorUserId;
        ChangeDescription = changeDescription;
        NewData = newData;
        Reason = reason;
        Status = ChangeRequestStatus.Pending;
    }

    /// <summary>Admin approves the request. Update logic is handled by the service layer.</summary>
    public void Approve(Guid reviewedByUserId, string? notes = null)
    {
        if (Status != ChangeRequestStatus.Pending)
            throw new InvalidOperationException("Only Pending requests can be approved.");

        Status = ChangeRequestStatus.Approved;
        ReviewedByUserId = reviewedByUserId;
        AdminNotes = notes;
        ReviewedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Admin rejects the request. No changes are applied.</summary>
    public void Reject(Guid reviewedByUserId, string? notes = null)
    {
        if (Status != ChangeRequestStatus.Pending)
            throw new InvalidOperationException("Only Pending requests can be rejected.");

        Status = ChangeRequestStatus.Rejected;
        ReviewedByUserId = reviewedByUserId;
        AdminNotes = notes;
        ReviewedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Requestor cancels their own request (before admin review).</summary>
    public void Cancel()
    {
        if (Status != ChangeRequestStatus.Pending)
            throw new InvalidOperationException("Only Pending requests can be cancelled.");

        Status = ChangeRequestStatus.Cancelled;
        Touch();
    }
}
