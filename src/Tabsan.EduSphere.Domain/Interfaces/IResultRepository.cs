using Tabsan.EduSphere.Domain.Assignments;

namespace Tabsan.EduSphere.Domain.Interfaces;

/// <summary>Repository interface for Result and TranscriptExportLog operations.</summary>
public interface IResultRepository
{
    // ── Results ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the result for the given student, offering, and type combination, or null.
    /// </summary>
    Task<Result?> GetAsync(Guid studentProfileId, Guid courseOfferingId, ResultType resultType, CancellationToken ct = default);

    /// <summary>Returns all results for the given student (published and draft).</summary>
    Task<IReadOnlyList<Result>> GetByStudentAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Returns published results only for the given student (student-visible view).</summary>
    Task<IReadOnlyList<Result>> GetPublishedByStudentAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Returns all results for a course offering (faculty/admin view, published and draft).</summary>
    Task<IReadOnlyList<Result>> GetByOfferingAsync(Guid courseOfferingId, CancellationToken ct = default);

    /// <summary>Returns true when a result row already exists for the given combination.</summary>
    Task<bool> ExistsAsync(Guid studentProfileId, Guid courseOfferingId, ResultType resultType, CancellationToken ct = default);

    /// <summary>Queues a new result for insertion.</summary>
    Task AddAsync(Result result, CancellationToken ct = default);

    /// <summary>Queues multiple results for bulk insertion (batch entry for an entire class).</summary>
    Task AddRangeAsync(IEnumerable<Result> results, CancellationToken ct = default);

    /// <summary>Marks a result as modified (publish or correction).</summary>
    void Update(Result result);

    // ── Transcript export logs ────────────────────────────────────────────────

    /// <summary>Returns all export log entries for the given student, ordered by export date descending.</summary>
    Task<IReadOnlyList<TranscriptExportLog>> GetExportLogsAsync(Guid studentProfileId, CancellationToken ct = default);

    /// <summary>Queues a new transcript export log entry for insertion.</summary>
    Task AddExportLogAsync(TranscriptExportLog log, CancellationToken ct = default);

    /// <summary>Commits pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
