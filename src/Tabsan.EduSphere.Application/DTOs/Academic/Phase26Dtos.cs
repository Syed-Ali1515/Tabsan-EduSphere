using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.DTOs.Academic;

// Phase 26 — School and College Functional Expansion

public sealed record SchoolStreamDto(Guid Id, string Name, string? Description, bool IsActive);

public sealed record SaveSchoolStreamRequest(string Name, string? Description, bool IsActive = true);

public sealed record StudentStreamAssignmentDto(
    Guid StudentProfileId,
    Guid StreamId,
    string StreamName,
    DateTime AssignedAt,
    Guid AssignedByUserId);

public sealed record AssignStudentStreamRequest(Guid StudentProfileId, Guid StreamId, Guid AssignedByUserId);

public sealed record GenerateReportCardRequest(
    Guid StudentProfileId,
    InstitutionType InstitutionType,
    string PeriodLabel,
    string PayloadJson,
    Guid GeneratedByUserId);

public sealed record ReportCardDto(
    Guid Id,
    Guid StudentProfileId,
    InstitutionType InstitutionType,
    string PeriodLabel,
    string PayloadJson,
    Guid GeneratedByUserId,
    DateTime GeneratedAt);

public sealed record CreateBulkPromotionBatchRequest(string Title, Guid CreatedByUserId);

public sealed record BulkPromotionEntryInput(Guid StudentProfileId, EntryDecision Decision, string? Reason = null);

public sealed record AddBulkPromotionEntriesRequest(Guid BatchId, IReadOnlyList<BulkPromotionEntryInput> Entries);

public sealed record ReviewBulkPromotionBatchRequest(Guid BatchId, Guid ReviewerUserId, bool Approve, string? Note);

public sealed record ApplyBulkPromotionBatchRequest(Guid BatchId);

public sealed record BulkPromotionEntryDto(
    Guid Id,
    Guid BatchId,
    Guid StudentProfileId,
    EntryDecision Decision,
    string? Reason,
    bool IsApplied,
    DateTime? AppliedAt);

public sealed record BulkPromotionBatchDto(
    Guid Id,
    string Title,
    BulkPromotionStatus Status,
    Guid CreatedByUserId,
    Guid? ApprovedByUserId,
    DateTime? ReviewedAt,
    DateTime? AppliedAt,
    string? ReviewNote,
    IReadOnlyList<BulkPromotionEntryDto> Entries);

public sealed record ParentLinkedStudentDto(
    Guid StudentProfileId,
    string RegistrationNumber,
    Guid ProgramId,
    Guid DepartmentId,
    int CurrentSemesterNumber,
    decimal Cgpa,
    decimal CurrentSemesterGpa,
    string? Relationship);

public sealed record UpsertParentStudentLinkRequest(
    Guid ParentUserId,
    Guid StudentProfileId,
    string? Relationship,
    bool IsActive = true);

public sealed record ParentStudentLinkDto(
    Guid ParentUserId,
    Guid StudentProfileId,
    string? Relationship,
    bool IsActive);
