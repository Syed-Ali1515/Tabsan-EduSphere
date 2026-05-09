using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 26 — Stage 26.2

public interface IReportCardService
{
    Task<ReportCardDto> GenerateAsync(GenerateReportCardRequest request, CancellationToken ct = default);

    Task<ReportCardDto?> GetLatestAsync(Guid studentProfileId, CancellationToken ct = default);

    Task<IReadOnlyList<ReportCardDto>> GetHistoryAsync(Guid studentProfileId, CancellationToken ct = default);
}
