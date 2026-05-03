using Tabsan.EduSphere.Application.DTOs.Assignments;

namespace Tabsan.EduSphere.Application.Interfaces;

public interface IResultCalculationService
{
    Task<ResultCalculationSettingsResponse> GetSettingsAsync(CancellationToken ct = default);
    Task SaveSettingsAsync(SaveResultCalculationSettingsRequest request, CancellationToken ct = default);
}