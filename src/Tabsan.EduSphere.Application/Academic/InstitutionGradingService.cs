using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Academic;

// Phase 25 — Academic Engine Unification — Stage 25.2

/// <summary>
/// Application service for managing institution-level grading profiles.
/// Provides upsert semantics: if a profile for the type already exists it is
/// updated; otherwise a new one is created.
/// </summary>
public class InstitutionGradingService : IInstitutionGradingService
{
    private readonly IInstitutionGradingProfileRepository _repo;

    public InstitutionGradingService(IInstitutionGradingProfileRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<InstitutionGradingProfileDto>> GetAllAsync(CancellationToken ct = default)
    {
        var profiles = await _repo.GetAllAsync(ct);
        return profiles.Select(ToDto).ToList();
    }

    public async Task<InstitutionGradingProfileDto?> GetByTypeAsync(InstitutionType institutionType, CancellationToken ct = default)
    {
        var profile = await _repo.GetByTypeAsync(institutionType, ct);
        return profile is null ? null : ToDto(profile);
    }

    public async Task<InstitutionGradingProfileDto> UpsertAsync(
        InstitutionType institutionType,
        SaveInstitutionGradingProfileRequest request,
        CancellationToken ct = default)
    {
        var existing = await _repo.GetByTypeAsync(institutionType, ct);

        if (existing is null)
        {
            var profile = new InstitutionGradingProfile(institutionType, request.PassThreshold, request.GradeRangesJson);
            if (!request.IsActive)
                profile.Update(request.PassThreshold, request.GradeRangesJson, false);
            await _repo.AddAsync(profile, ct);
            await _repo.SaveChangesAsync(ct);
            return ToDto(profile);
        }

        existing.Update(request.PassThreshold, request.GradeRangesJson, request.IsActive);
        _repo.Update(existing);
        await _repo.SaveChangesAsync(ct);
        return ToDto(existing);
    }

    private static InstitutionGradingProfileDto ToDto(InstitutionGradingProfile p)
        => new(p.Id, p.InstitutionType, p.PassThreshold, p.GradeRangesJson, p.IsActive);
}
