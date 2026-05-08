// Final-Touches Phase 18 Stage 18.1 — Graduation application entity with multi-stage approval workflow

using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Academic;

/// <summary>
/// Lifecycle status of a student's graduation application.
/// Transitions: Draft → PendingFaculty → PendingAdmin → PendingFinalApproval → Approved / Rejected.
/// </summary>
public enum GraduationApplicationStatus
{
    /// <summary>Saved but not yet submitted.</summary>
    Draft = 1,

    /// <summary>Submitted; awaiting Faculty advisor review.</summary>
    PendingFaculty = 2,

    /// <summary>Faculty approved; awaiting departmental Admin approval.</summary>
    PendingAdmin = 3,

    /// <summary>Admin approved; awaiting final SuperAdmin confirmation.</summary>
    PendingFinalApproval = 4,

    /// <summary>SuperAdmin confirmed — student is cleared to graduate.</summary>
    Approved = 5,

    /// <summary>Rejected at any stage — student may resubmit after addressing issues.</summary>
    Rejected = 6
}

/// <summary>
/// Stage in the approval chain at which a rejection or note was recorded.
/// </summary>
public enum ApprovalStage
{
    Faculty = 1,
    Admin = 2,
    SuperAdmin = 3
}

/// <summary>
/// A student's formal request to graduate.
/// Supports a three-stage approval chain (Faculty → Admin → SuperAdmin).
/// One active application per student at a time.
/// </summary>
public class GraduationApplication : AuditableEntity
{
    // Final-Touches Phase 18 Stage 18.1 — core FK fields
    /// <summary>FK to the student's academic profile.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>Navigation to the student profile.</summary>
    public StudentProfile StudentProfile { get; private set; } = default!;

    /// <summary>Current status in the approval workflow.</summary>
    public GraduationApplicationStatus Status { get; private set; } = GraduationApplicationStatus.Draft;

    /// <summary>Optional cover note from the student.</summary>
    public string? StudentNote { get; private set; }

    /// <summary>UTC date the student submitted the application (moved from Draft → PendingFaculty).</summary>
    public DateTime? SubmittedAt { get; private set; }

    // Final-Touches Phase 18 Stage 18.2 — certificate path stored on approval
    /// <summary>
    /// Relative path (under wwwroot) to the generated graduation certificate PDF.
    /// Set when the application reaches Approved status.
    /// </summary>
    public string? CertificatePath { get; private set; }

    /// <summary>UTC timestamp when the certificate was generated.</summary>
    public DateTime? CertificateGeneratedAt { get; private set; }

    private readonly List<GraduationApplicationApproval> _approvals = new();
    /// <summary>Per-stage approval / rejection records.</summary>
    public IReadOnlyList<GraduationApplicationApproval> Approvals => _approvals.AsReadOnly();

    protected GraduationApplication() { }

    // Final-Touches Phase 18 Stage 18.1 — static factory
    /// <summary>Creates a new graduation application in Draft status.</summary>
    public static GraduationApplication Create(Guid studentProfileId, string? studentNote)
        => new()
        {
            Id               = Guid.NewGuid(),
            StudentProfileId = studentProfileId,
            Status           = GraduationApplicationStatus.Draft,
            StudentNote      = studentNote?.Trim(),
            CreatedAt        = DateTime.UtcNow
        };

    // Final-Touches Phase 18 Stage 18.1 — state transitions
    /// <summary>Submits the application for Faculty review.</summary>
    public void Submit()
    {
        if (Status != GraduationApplicationStatus.Draft)
            throw new InvalidOperationException("Only a Draft application can be submitted.");
        Status      = GraduationApplicationStatus.PendingFaculty;
        SubmittedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Faculty approves — moves to Admin review.</summary>
    public void FacultyApprove(Guid approverUserId, string? note)
    {
        if (Status != GraduationApplicationStatus.PendingFaculty)
            throw new InvalidOperationException("Application is not pending Faculty review.");
        _approvals.Add(GraduationApplicationApproval.Create(Id, ApprovalStage.Faculty, approverUserId, true, note));
        Status = GraduationApplicationStatus.PendingAdmin;
        Touch();
    }

    /// <summary>Admin approves — moves to SuperAdmin final approval.</summary>
    public void AdminApprove(Guid approverUserId, string? note)
    {
        if (Status != GraduationApplicationStatus.PendingAdmin)
            throw new InvalidOperationException("Application is not pending Admin review.");
        _approvals.Add(GraduationApplicationApproval.Create(Id, ApprovalStage.Admin, approverUserId, true, note));
        Status = GraduationApplicationStatus.PendingFinalApproval;
        Touch();
    }

    /// <summary>SuperAdmin gives final approval — application is Approved.</summary>
    public void FinalApprove(Guid approverUserId, string? note)
    {
        if (Status != GraduationApplicationStatus.PendingFinalApproval)
            throw new InvalidOperationException("Application is not pending final approval.");
        _approvals.Add(GraduationApplicationApproval.Create(Id, ApprovalStage.SuperAdmin, approverUserId, true, note));
        Status = GraduationApplicationStatus.Approved;
        Touch();
    }

    /// <summary>Rejects the application at any stage.</summary>
    public void Reject(Guid approverUserId, ApprovalStage stage, string? reason)
    {
        _approvals.Add(GraduationApplicationApproval.Create(Id, stage, approverUserId, false, reason));
        Status = GraduationApplicationStatus.Rejected;
        Touch();
    }

    // Final-Touches Phase 18 Stage 18.2 — store certificate path after PDF generation
    /// <summary>Records the path of the generated certificate PDF.</summary>
    public void AttachCertificate(string relativePath)
    {
        CertificatePath         = relativePath;
        CertificateGeneratedAt  = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Clears the certificate path (used for re-issue).</summary>
    public void ClearCertificate()
    {
        CertificatePath        = null;
        CertificateGeneratedAt = null;
        Touch();
    }
}

/// <summary>
/// Immutable record of one approver action (approve or reject) at a specific stage.
/// </summary>
public class GraduationApplicationApproval : BaseEntity
{
    // Final-Touches Phase 18 Stage 18.1 — per-stage approval record
    public Guid                    GraduationApplicationId { get; private set; }
    public ApprovalStage           Stage                   { get; private set; }
    public Guid                    ApproverUserId          { get; private set; }
    public bool                    IsApproved              { get; private set; }
    public string?                 Note                    { get; private set; }
    public DateTime                ActedAt                 { get; private set; }

    protected GraduationApplicationApproval() { }

    internal static GraduationApplicationApproval Create(
        Guid applicationId, ApprovalStage stage, Guid approverUserId, bool isApproved, string? note)
        => new()
        {
            Id                      = Guid.NewGuid(),
            GraduationApplicationId = applicationId,
            Stage                   = stage,
            ApproverUserId          = approverUserId,
            IsApproved              = isApproved,
            Note                    = note?.Trim(),
            ActedAt                 = DateTime.UtcNow,
            CreatedAt               = DateTime.UtcNow
        };
}
