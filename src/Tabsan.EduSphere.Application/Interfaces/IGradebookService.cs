// Final-Touches Phase 16 Stage 16.1/16.3 — gradebook service interface

using Tabsan.EduSphere.Application.DTOs.Assignments;

namespace Tabsan.EduSphere.Application.Interfaces;

public interface IGradebookService
{
    // Final-Touches Phase 16 Stage 16.1 — fetch complete gradebook grid
    Task<GradebookGridResponse> GetGradebookAsync(Guid courseOfferingId, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.1 — upsert one result cell (create or correct)
    Task UpsertEntryAsync(UpsertGradebookEntryRequest request, Guid facultyUserId, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.1 — publish all results for the offering
    Task PublishAllAsync(Guid courseOfferingId, Guid facultyUserId, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.3 — generate CSV template bytes for a component
    Task<byte[]> GetCsvTemplateAsync(Guid courseOfferingId, string componentName, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.3 — parse uploaded CSV and return a preview
    Task<BulkGradePreviewResponse> ParseBulkCsvAsync(Guid courseOfferingId, string componentName, Stream csvStream, CancellationToken ct = default);

    // Final-Touches Phase 16 Stage 16.3 — apply confirmed bulk-grade rows
    Task ConfirmBulkGradeAsync(BulkGradeConfirmRequest request, Guid facultyUserId, CancellationToken ct = default);
}
