using Tabsan.EduSphere.Domain.Academic;

namespace Tabsan.EduSphere.Domain.Interfaces;

// Phase 26 — Stage 26.2

/// <summary>Repository abstraction for bulk promotion batches.</summary>
public interface IBulkPromotionRepository
{
    Task AddBatchAsync(BulkPromotionBatch batch, CancellationToken ct = default);

    Task<BulkPromotionBatch?> GetBatchByIdAsync(Guid id, CancellationToken ct = default);

    Task<IReadOnlyList<BulkPromotionBatch>> GetRecentBatchesAsync(int take = 20, CancellationToken ct = default);

    void UpdateBatch(BulkPromotionBatch batch);

    Task AddEntryAsync(BulkPromotionEntry entry, CancellationToken ct = default);

    Task<IReadOnlyList<BulkPromotionEntry>> GetEntriesAsync(Guid batchId, CancellationToken ct = default);

    void UpdateEntry(BulkPromotionEntry entry);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
