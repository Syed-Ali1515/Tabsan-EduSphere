// Final-Touches Phase 18 Stage 18.1/18.2 — Graduation workflow DTOs

namespace Tabsan.EduSphere.Application.DTOs.Academic;

// ── Requests ─────────────────────────────────────────────────────────────────

// Final-Touches Phase 18 Stage 18.1 — student submits application
/// <summary>Request body for a student creating/submitting a graduation application.</summary>
public sealed record SubmitGraduationApplicationRequest(
    string? StudentNote
);

// Final-Touches Phase 18 Stage 18.1 — approver action
/// <summary>Request body for an approver action (approve or reject).</summary>
public sealed record GraduationApprovalRequest(
    bool   IsApproved,
    string? Note
);

// ── Responses ─────────────────────────────────────────────────────────────────

// Final-Touches Phase 18 Stage 18.1 — application summary for lists
/// <summary>Summary row for a graduation application list.</summary>
public sealed record GraduationApplicationSummary(
    Guid   Id,
    Guid   StudentProfileId,
    string StudentName,
    string RegistrationNumber,
    string ProgramName,
    string Status,
    DateTime? SubmittedAt,
    DateTime? UpdatedAt,
    bool   HasCertificate
);

/// <summary>Paged response wrapper for graduation application lists.</summary>
public sealed record GraduationApplicationPageDto(
    IReadOnlyList<GraduationApplicationSummary> Items,
    int Page,
    int PageSize,
    int TotalCount
);

// Final-Touches Phase 18 Stage 18.1/18.2 — full application detail
/// <summary>Full detail of a graduation application including approval history.</summary>
public sealed record GraduationApplicationDetail(
    Guid   Id,
    Guid   StudentProfileId,
    string StudentName,
    string RegistrationNumber,
    string ProgramName,
    string Status,
    string? StudentNote,
    DateTime? SubmittedAt,
    DateTime? UpdatedAt,
    bool   HasCertificate,
    string? CertificatePath,
    IReadOnlyList<ApprovalHistoryItem> ApprovalHistory
);

// Final-Touches Phase 18 Stage 18.1 — individual approval record
/// <summary>One entry in the approval chain history.</summary>
public sealed record ApprovalHistoryItem(
    string Stage,
    string ApproverName,
    bool   IsApproved,
    string? Note,
    DateTime ActedAt
);
