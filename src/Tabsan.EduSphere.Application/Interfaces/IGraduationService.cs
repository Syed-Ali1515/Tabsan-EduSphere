// Final-Touches Phase 18 Stage 18.1/18.2 — Graduation service interface

using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application service interface for the Graduation Workflow.
/// Covers application lifecycle (Stage 18.1) and certificate generation (Stage 18.2).
/// </summary>
public interface IGraduationService
{
    // Final-Touches Phase 18 Stage 18.1 — application lifecycle

    /// <summary>Returns a paged graduation application list for the authenticated student.</summary>
    Task<GraduationApplicationPageDto> GetMyApplicationsAsync(Guid studentProfileId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Returns the full detail of a single application.</summary>
    Task<GraduationApplicationDetail> GetApplicationDetailAsync(Guid applicationId, CancellationToken ct = default);

    /// <summary>
    /// Returns applications visible to an Admin (dept-scoped) or SuperAdmin (all).
    /// Pass departmentId = null for SuperAdmin full view.
    /// </summary>
    Task<GraduationApplicationPageDto> GetApplicationsAsync(Guid? departmentId, string? statusFilter, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Creates a graduation application in Draft status and submits it for Faculty review.</summary>
    Task<GraduationApplicationSummary> SubmitApplicationAsync(Guid studentProfileId, SubmitGraduationApplicationRequest request, CancellationToken ct = default);

    /// <summary>Faculty approves the application — advances to Admin review.</summary>
    Task<GraduationApplicationSummary> FacultyApproveAsync(Guid applicationId, Guid approverUserId, GraduationApprovalRequest request, CancellationToken ct = default);

    /// <summary>Admin approves the application — advances to SuperAdmin final approval.</summary>
    Task<GraduationApplicationSummary> AdminApproveAsync(Guid applicationId, Guid approverUserId, GraduationApprovalRequest request, CancellationToken ct = default);

    /// <summary>SuperAdmin gives final approval; triggers certificate generation and graduation.</summary>
    Task<GraduationApplicationSummary> FinalApproveAsync(Guid applicationId, Guid approverUserId, GraduationApprovalRequest request, CancellationToken ct = default);

    /// <summary>Rejects the application at any stage with an optional reason.</summary>
    Task<GraduationApplicationSummary> RejectAsync(Guid applicationId, Guid approverUserId, string approverRole, GraduationApprovalRequest request, CancellationToken ct = default);

    // Final-Touches Phase 18 Stage 18.2 — certificate management

    /// <summary>
    /// Generates (or re-generates) the graduation certificate PDF and stores the storage key reference.
    /// Returns the persisted storage key.
    /// </summary>
    Task<string> GenerateCertificateAsync(Guid applicationId, CancellationToken ct = default);

    /// <summary>
    /// Returns the byte content of the certificate PDF for download.
    /// Returns null when the certificate does not exist.
    /// </summary>
    Task<byte[]?> DownloadCertificateAsync(Guid applicationId, Guid requestingStudentProfileId, CancellationToken ct = default);
}
