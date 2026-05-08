using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 25 — Academic Engine Unification — Stage 25.2

/// <summary>
/// Application service for managing institution-level grading profiles.
/// SuperAdmin can create and update profiles for each institution type.
/// The profiles are used as defaults when no per-course grading config exists.
/// </summary>
public interface IInstitutionGradingService
{
    /// <summary>Returns all institution grading profiles.</summary>
    Task<IReadOnlyList<InstitutionGradingProfileDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>Returns the active grading profile for the given institution type, or null.</summary>
    Task<InstitutionGradingProfileDto?> GetByTypeAsync(InstitutionType institutionType, CancellationToken ct = default);

    /// <summary>
    /// Creates or updates the grading profile for the given institution type.
    /// If no profile exists for the type it is created; otherwise the existing one is updated.
    /// </summary>
    Task<InstitutionGradingProfileDto> UpsertAsync(
        InstitutionType institutionType,
        SaveInstitutionGradingProfileRequest request,
        CancellationToken ct = default);
}
