using Tabsan.EduSphere.Application.DTOs.Academic;

namespace Tabsan.EduSphere.Application.Interfaces;

// Phase 26 — Stage 26.2

public interface IBulkPromotionService
{
    Task<BulkPromotionBatchDto> CreateBatchAsync(CreateBulkPromotionBatchRequest request, CancellationToken ct = default);

    Task<BulkPromotionBatchDto> AddEntriesAsync(AddBulkPromotionEntriesRequest request, CancellationToken ct = default);

    Task<BulkPromotionBatchDto> SubmitAsync(Guid batchId, CancellationToken ct = default);

    Task<BulkPromotionBatchDto> ReviewAsync(ReviewBulkPromotionBatchRequest request, CancellationToken ct = default);

    Task<BulkPromotionBatchDto> ApplyAsync(ApplyBulkPromotionBatchRequest request, CancellationToken ct = default);

    Task<BulkPromotionBatchDto?> GetByIdAsync(Guid batchId, CancellationToken ct = default);
}
