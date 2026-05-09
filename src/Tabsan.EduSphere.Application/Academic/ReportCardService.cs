using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Academic;

// Phase 26 — Stage 26.2

public class ReportCardService : IReportCardService
{
    private readonly IReportCardRepository _repo;

    public ReportCardService(IReportCardRepository repo)
    {
        _repo = repo;
    }

    public async Task<ReportCardDto> GenerateAsync(GenerateReportCardRequest request, CancellationToken ct = default)
    {
        var card = new StudentReportCard(
            request.StudentProfileId,
            request.InstitutionType,
            request.PeriodLabel,
            request.PayloadJson,
            request.GeneratedByUserId);

        await _repo.AddAsync(card, ct);
        await _repo.SaveChangesAsync(ct);

        return ToDto(card);
    }

    public async Task<ReportCardDto?> GetLatestAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var card = await _repo.GetLatestForStudentAsync(studentProfileId, ct);
        return card is null ? null : ToDto(card);
    }

    public async Task<IReadOnlyList<ReportCardDto>> GetHistoryAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var cards = await _repo.GetForStudentAsync(studentProfileId, ct);
        return cards.Select(ToDto).ToList();
    }

    private static ReportCardDto ToDto(StudentReportCard card)
        => new(
            card.Id,
            card.StudentProfileId,
            card.InstitutionType,
            card.PeriodLabel,
            card.PayloadJson,
            card.GeneratedByUserId,
            card.GeneratedAt);
}
