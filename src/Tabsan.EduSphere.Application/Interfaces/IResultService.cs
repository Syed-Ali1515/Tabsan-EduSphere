using Tabsan.EduSphere.Application.DTOs.Assignments;
using Tabsan.EduSphere.Domain.Assignments;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Contract for result entry, publication, correction, and transcript export.
/// Faculty can enter and publish results for their own offerings.
/// Admins can publish, bulk-enter, and correct published results.
/// Students can view published results and request transcript exports.
/// </summary>
public interface IResultService
{
    // ── Result entry ──────────────────────────────────────────────────────────

    /// <summary>Creates a draft result entry (not yet visible to the student).</summary>
    Task<ResultResponse> CreateAsync(CreateResultRequest request, CancellationToken ct = default);

    /// <summary>Bulk-creates draft result entries for an entire class in one transaction.</summary>
    Task<int> BulkCreateAsync(BulkCreateResultsRequest request, CancellationToken ct = default);

    // ── Publication ───────────────────────────────────────────────────────────

    /// <summary>
    /// Publishes a single result, making it visible to the student.
    /// Publication is a one-way operation — use CorrectAsync for Admin corrections.
    /// </summary>
    Task<bool> PublishAsync(Guid studentProfileId, Guid courseOfferingId, ResultType resultType, Guid publishedByUserId, CancellationToken ct = default);

    /// <summary>
    /// Publishes all draft results for a course offering in one operation.
    /// Returns the count of results published.
    /// </summary>
    Task<int> PublishAllForOfferingAsync(Guid courseOfferingId, Guid publishedByUserId, CancellationToken ct = default);

    // ── Corrections (Admin only) ──────────────────────────────────────────────

    /// <summary>
    /// Admin-only override to correct an already-published result.
    /// The service must log the correction in the AuditLog before applying changes.
    /// </summary>
    Task<bool> CorrectAsync(Guid studentProfileId, Guid courseOfferingId, ResultType resultType, CorrectResultRequest request, Guid correctedByUserId, CancellationToken ct = default);

    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>Returns all results for a student (published + draft) — faculty/admin view.</summary>
    Task<IReadOnlyList<ResultResponse>> GetByStudentAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Returns only published results for a student — student-visible view.</summary>
    Task<IReadOnlyList<ResultResponse>> GetPublishedByStudentAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Returns all results (draft and published) for a course offering.</summary>
    Task<IReadOnlyList<ResultResponse>> GetByOfferingAsync(Guid courseOfferingId, CancellationToken ct = default);

    // ── Transcript ────────────────────────────────────────────────────────────

    /// <summary>
    /// Generates a transcript for the student, logs the export, and returns the result set.
    /// All published results are included regardless of semester.
    /// </summary>
    Task<(IReadOnlyList<ResultResponse> Results, Guid LogId)> ExportTranscriptAsync(
        TranscriptExportRequest request, Guid requestedByUserId, string? ipAddress, CancellationToken ct = default);

    /// <summary>Returns the export history for a student.</summary>
    Task<IReadOnlyList<TranscriptExportLogResponse>> GetExportHistoryAsync(Guid studentProfileId, CancellationToken ct = default);
}
