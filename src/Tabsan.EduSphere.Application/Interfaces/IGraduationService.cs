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

    /// <summary>Returns all graduation applications for the authenticated student.</summary>
    Task<IReadOnlyList<GraduationApplicationSummary>> GetMyApplicationsAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Returns the full detail of a single application.</summary>
    Task<GraduationApplicationDetail> GetApplicationDetailAsync(Guid applicationId, CancellationToken ct = default);

    /// <summary>
    /// Returns applications visible to an Admin (dept-scoped) or SuperAdmin (all).
    /// Pass departmentId = null for SuperAdmin full view.
    /// </summary>
    Task<IReadOnlyList<GraduationApplicationSummary>> GetApplicationsAsync(Guid? departmentId, string? statusFilter, CancellationToken ct = default);

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
    /// Generates (or re-generates) the graduation certificate PDF and stores the path.
    /// Returns the certificate file path relative to wwwroot.
    /// </summary>
    Task<string> GenerateCertificateAsync(Guid applicationId, CancellationToken ct = default);

    /// <summary>
    /// Returns the byte content of the certificate PDF for download.
    /// Returns null when the certificate does not exist.
    /// </summary>
    Task<byte[]?> DownloadCertificateAsync(Guid applicationId, Guid requestingStudentProfileId, CancellationToken ct = default);
}
