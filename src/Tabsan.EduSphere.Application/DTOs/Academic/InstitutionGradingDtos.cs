using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.DTOs.Academic;

// Phase 25 — Academic Engine Unification — Stage 25.2

/// <summary>Response DTO for an institution grading profile.</summary>
public sealed record InstitutionGradingProfileDto(
    Guid Id,
    InstitutionType InstitutionType,
    decimal PassThreshold,
    string? GradeRangesJson,
    bool IsActive);

/// <summary>Request DTO to create or update an institution grading profile.</summary>
public sealed record SaveInstitutionGradingProfileRequest(
    decimal PassThreshold,
    string? GradeRangesJson,
    bool IsActive = true);
