namespace Tabsan.EduSphere.Domain.Enums;

/// <summary>
/// Represents the status of an admin change request.
/// Students and teachers may request changes to their profiles (name, address, etc.).
/// Admins approve or reject these requests.
/// </summary>
public enum ChangeRequestStatus
{
    /// <summary>Request submitted, awaiting admin review.</summary>
    Pending = 1,

    /// <summary>Admin has approved the change; update has been applied.</summary>
    Approved = 2,

    /// <summary>Admin has rejected the change; no update applied.</summary>
    Rejected = 3,

    /// <summary>Request was cancelled by the requestor before admin review.</summary>
    Cancelled = 4
}
