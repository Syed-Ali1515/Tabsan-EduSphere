using Tabsan.EduSphere.Application.DTOs.Assignments;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Assignments;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Application.Assignments;

/// <summary>
/// Orchestrates result entry, publication, Admin correction, and transcript export.
/// Business invariants:
///   - Results are per (student, offering, type) — no duplicates.
///   - Publication is one-way; corrections require an Admin and are always audited.
///   - Transcript export creates a TranscriptExportLog row for every request.
///   - Faculty can only publish results for their own course offerings (enforced at controller layer).
/// </summary>
public class ResultService : IResultService
{
    private readonly IResultRepository _repo;
    private readonly IAuditService _audit;

    public ResultService(IResultRepository repo, IAuditService audit)
    {
        _repo = repo;
        _audit = audit;
    }

    // ── Result entry ──────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a draft result entry for a single student.
    /// Throws when a result already exists for the same (student, offering, type) combination.
    /// </summary>
    public async Task<ResultResponse> CreateAsync(CreateResultRequest request, CancellationToken ct = default)
    {
        if (!Enum.TryParse<ResultType>(request.ResultType, ignoreCase: true, out var resultType))
            throw new ArgumentException($"Invalid result type '{request.ResultType}'.");

        if (await _repo.ExistsAsync(request.StudentProfileId, request.CourseOfferingId, resultType, ct))
            throw new InvalidOperationException("A result entry already exists for this student / offering / type.");

        var result = new Result(request.StudentProfileId, request.CourseOfferingId, resultType,
                                request.MarksObtained, request.MaxMarks);
        await _repo.AddAsync(result, ct);
        await _repo.SaveChangesAsync(ct);
        return ToResponse(result);
    }

    /// <summary>
    /// Bulk-creates draft result entries for an entire class.
    /// Skips any entries that already exist rather than throwing.
    /// Returns the count of newly inserted rows.
    /// </summary>
    public async Task<int> BulkCreateAsync(BulkCreateResultsRequest request, CancellationToken ct = default)
    {
        var toInsert = new List<Result>();
        foreach (var r in request.Results)
        {
            if (!Enum.TryParse<ResultType>(r.ResultType, ignoreCase: true, out var resultType))
                continue;

            // Skip existing — do not overwrite.
            if (await _repo.ExistsAsync(r.StudentProfileId, r.CourseOfferingId, resultType, ct))
                continue;

            toInsert.Add(new Result(r.StudentProfileId, r.CourseOfferingId, resultType, r.MarksObtained, r.MaxMarks));
        }

        if (toInsert.Count == 0) return 0;

        await _repo.AddRangeAsync(toInsert, ct);
        await _repo.SaveChangesAsync(ct);
        return toInsert.Count;
    }

    // ── Publication ───────────────────────────────────────────────────────────

    /// <summary>
    /// Publishes a single result, making it visible to the student.
    /// Returns false when the result does not exist or is already published.
    /// </summary>
    public async Task<bool> PublishAsync(Guid studentProfileId, Guid courseOfferingId, ResultType resultType,
                                         Guid publishedByUserId, CancellationToken ct = default)
    {
        var result = await _repo.GetAsync(studentProfileId, courseOfferingId, resultType, ct);
        if (result is null) return false;

        try { result.Publish(publishedByUserId); }
        catch (InvalidOperationException) { return false; }

        _repo.Update(result);
        await _repo.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog("PublishResult", "Result", result.Id.ToString(),
            actorUserId: publishedByUserId), ct);

        return true;
    }

    /// <summary>
    /// Publishes all draft results for a course offering in one batch.
    /// Already-published results are silently skipped.
    /// Returns the number of results newly published.
    /// </summary>
    public async Task<int> PublishAllForOfferingAsync(Guid courseOfferingId, Guid publishedByUserId, CancellationToken ct = default)
    {
        var results = await _repo.GetByOfferingAsync(courseOfferingId, ct);
        var unpublished = results.Where(r => !r.IsPublished).ToList();

        foreach (var result in unpublished)
            result.Publish(publishedByUserId);

        foreach (var result in unpublished)
            _repo.Update(result);

        if (unpublished.Count > 0)
        {
            await _repo.SaveChangesAsync(ct);
            await _audit.LogAsync(new AuditLog("BulkPublishResults", "CourseOffering", courseOfferingId.ToString(),
                actorUserId: publishedByUserId,
                newValuesJson: $"{{\"count\":{unpublished.Count}}}"), ct);
        }

        return unpublished.Count;
    }

    // ── Corrections (Admin only) ──────────────────────────────────────────────

    /// <summary>
    /// Applies an Admin-authorised correction to a published result.
    /// Captures the old values in the audit log before overwriting.
    /// Returns false when the result does not exist.
    /// </summary>
    public async Task<bool> CorrectAsync(Guid studentProfileId, Guid courseOfferingId, ResultType resultType,
                                          CorrectResultRequest request, Guid correctedByUserId, CancellationToken ct = default)
    {
        var result = await _repo.GetAsync(studentProfileId, courseOfferingId, resultType, ct);
        if (result is null) return false;

        var oldJson = $"{{\"marks\":{result.MarksObtained},\"max\":{result.MaxMarks}}}";

        result.CorrectMarks(request.NewMarksObtained, request.NewMaxMarks);
        _repo.Update(result);
        await _repo.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog("CorrectResult", "Result", result.Id.ToString(),
            actorUserId: correctedByUserId,
            oldValuesJson: oldJson,
            newValuesJson: $"{{\"marks\":{request.NewMarksObtained},\"max\":{request.NewMaxMarks}}}"), ct);

        return true;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>Returns all results for a student (draft + published) for faculty/admin views.</summary>
    public async Task<IReadOnlyList<ResultResponse>> GetByStudentAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var results = await _repo.GetByStudentAsync(studentProfileId, ct);
        return results.Select(ToResponse).ToList();
    }

    /// <summary>Returns only published results for a student (what the student sees).</summary>
    public async Task<IReadOnlyList<ResultResponse>> GetPublishedByStudentAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var results = await _repo.GetPublishedByStudentAsync(studentProfileId, ct);
        return results.Select(ToResponse).ToList();
    }

    /// <summary>Returns all results (draft + published) for a course offering.</summary>
    public async Task<IReadOnlyList<ResultResponse>> GetByOfferingAsync(Guid courseOfferingId, CancellationToken ct = default)
    {
        var results = await _repo.GetByOfferingAsync(courseOfferingId, ct);
        return results.Select(ToResponse).ToList();
    }

    // ── Transcript ────────────────────────────────────────────────────────────

    /// <summary>
    /// Gathers all published results for the student, logs the export request,
    /// and returns the results along with the new log entry ID.
    /// </summary>
    public async Task<(IReadOnlyList<ResultResponse> Results, Guid LogId)> ExportTranscriptAsync(
        TranscriptExportRequest request, Guid requestedByUserId, string? ipAddress, CancellationToken ct = default)
    {
        var results = await _repo.GetPublishedByStudentAsync(request.StudentProfileId, ct);
        var resultDtos = results.Select(ToResponse).ToList();

        var log = new TranscriptExportLog(request.StudentProfileId, requestedByUserId, request.Format, ipAddress: ipAddress);
        await _repo.AddExportLogAsync(log, ct);
        await _repo.SaveChangesAsync(ct);

        await _audit.LogAsync(new AuditLog("ExportTranscript", "StudentProfile", request.StudentProfileId.ToString(),
            actorUserId: requestedByUserId, ipAddress: ipAddress), ct);

        return (resultDtos, log.Id);
    }

    /// <summary>Returns the transcript export history for a student, newest first.</summary>
    public async Task<IReadOnlyList<TranscriptExportLogResponse>> GetExportHistoryAsync(Guid studentProfileId, CancellationToken ct = default)
    {
        var logs = await _repo.GetExportLogsAsync(studentProfileId, ct);
        return logs.Select(l => new TranscriptExportLogResponse(l.Id, l.ExportedAt, l.Format, l.DocumentUrl)).ToList();
    }

    // ── Mapping helpers ───────────────────────────────────────────────────────

    /// <summary>Maps a domain Result to a ResultResponse DTO.</summary>
    private static ResultResponse ToResponse(Result r) =>
        new(r.Id, r.StudentProfileId, r.CourseOfferingId,
            r.ResultType.ToString(),
            r.MarksObtained, r.MaxMarks,
            r.MaxMarks > 0 ? Math.Round(r.MarksObtained / r.MaxMarks * 100, 2) : 0,
            r.IsPublished, r.PublishedAt);
}
