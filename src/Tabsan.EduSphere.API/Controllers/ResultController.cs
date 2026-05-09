using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.API.Services;
using Tabsan.EduSphere.Application.DTOs.Assignments;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages result entry, publication, Admin corrections, and transcript exports.
/// Faculty: enter and publish results for their own offerings.
/// Admins: bulk-enter, publish all, and correct results.
/// Students: view own published results and request transcript exports.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ResultController : ControllerBase
{
    private readonly IResultService _service;
    private readonly ResultPublishJobQueue _publishQueue;
    private readonly ResultPublishJobStore _publishStore;

    public ResultController(IResultService service, ResultPublishJobQueue publishQueue, ResultPublishJobStore publishStore)
    {
        _service = service;
        _publishQueue = publishQueue;
        _publishStore = publishStore;
    }

    // ── Result entry ──────────────────────────────────────────────────────────

    /// <summary>Creates a single draft result entry (Faculty/Admin).</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> Create([FromBody] CreateResultRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.CreateAsync(request, ct);
            return Ok(result);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    /// <summary>Bulk-creates draft result entries for an entire class in one call (Faculty/Admin).</summary>
    [HttpPost("bulk")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> BulkCreate([FromBody] BulkCreateResultsRequest request, CancellationToken ct)
    {
        var count = await _service.BulkCreateAsync(request, ct);
        return Ok(new { inserted = count });
    }

    // ── Publication ───────────────────────────────────────────────────────────

    /// <summary>Publishes a single result, making it visible to the student (Faculty/Admin).</summary>
    [HttpPost("publish")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> Publish(
        [FromQuery] Guid studentProfileId,
        [FromQuery] Guid courseOfferingId,
        [FromQuery] string resultType,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var ok = await _service.PublishAsync(studentProfileId, courseOfferingId, resultType, userId, ct);
        return ok ? NoContent() : BadRequest("Result not found or already published.");
    }

    /// <summary>Publishes all draft results for a course offering in one batch (Faculty/Admin).</summary>
    [HttpPost("publish-all")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> PublishAll([FromQuery] Guid courseOfferingId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var count = await _service.PublishAllForOfferingAsync(courseOfferingId, userId, ct);
        return Ok(new { published = count });
    }

    /// <summary>
    /// Queues a publish-all operation for asynchronous execution.
    /// Use the returned job ID to poll status.
    /// </summary>
    [HttpPost("publish-all/jobs")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> QueuePublishAll([FromQuery] Guid courseOfferingId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();
        if (courseOfferingId == Guid.Empty) return BadRequest("courseOfferingId is required.");

        var jobId = Guid.NewGuid();
        await _publishStore.SetAsync(new ResultPublishJobState
        {
            JobId = jobId,
            CourseOfferingId = courseOfferingId,
            RequestedByUserId = userId,
            Status = "queued"
        }, ct);

        _publishQueue.Enqueue(new ResultPublishJobWorkItem(jobId, courseOfferingId, userId));
        return Accepted(new
        {
            jobId,
            status = "queued",
            statusUrl = $"/api/v1/result/publish-all/jobs/{jobId}"
        });
    }

    /// <summary>Returns status for an asynchronous publish-all job.</summary>
    [HttpGet("publish-all/jobs/{jobId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetPublishAllJob(Guid jobId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var state = await _publishStore.GetAsync(jobId, ct);
        if (state is null) return NotFound();
        if (state.RequestedByUserId != userId && !User.IsInRole("SuperAdmin") && !User.IsInRole("Admin"))
            return Forbid();

        return Ok(state);
    }

    // ── Admin correction ──────────────────────────────────────────────────────

    /// <summary>
    /// Corrects an already-published result (Admin only).
    /// Creates an audit trail before applying changes.
    /// </summary>
    [HttpPut("correct")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Correct(
        [FromQuery] Guid studentProfileId,
        [FromQuery] Guid courseOfferingId,
        [FromQuery] string resultType,
        [FromBody] CorrectResultRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var ok = await _service.CorrectAsync(studentProfileId, courseOfferingId, resultType, request, userId, ct);
            return ok ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>Returns all published results for the current student (Student view).</summary>
    [HttpGet("my-results")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyResults(CancellationToken ct)
    {
        var studentProfileId = GetCurrentStudentProfileId();
        if (studentProfileId == Guid.Empty) return Unauthorized();

        var results = await _service.GetPublishedByStudentAsync(studentProfileId, ct);
        return Ok(results);
    }

    /// <summary>Returns all results (draft + published) for a student (Faculty/Admin view).</summary>
    [HttpGet("by-student/{studentProfileId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetByStudent(Guid studentProfileId, CancellationToken ct)
    {
        var results = await _service.GetByStudentAsync(studentProfileId, ct);
        return Ok(results);
    }

    /// <summary>Returns all results for a course offering (Faculty/Admin view).</summary>
    [HttpGet("by-offering/{courseOfferingId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetByOffering(Guid courseOfferingId, CancellationToken ct)
    {
        var results = await _service.GetByOfferingAsync(courseOfferingId, ct);
        return Ok(results);
    }

    // ── Transcript ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all published results for a student as a transcript payload.
    /// Logs the export in TranscriptExportLog and AuditLog.
    /// </summary>
    [HttpGet("transcript/{studentProfileId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty,Student")]
    public async Task<IActionResult> GetTranscript(Guid studentProfileId, [FromQuery] string format = "PDF", CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        var request = new TranscriptExportRequest(studentProfileId, format.ToUpperInvariant());
        var (results, logId) = await _service.ExportTranscriptAsync(request, userId, ip, ct);
        return Ok(new { logId, results });
    }

    /// <summary>Returns a student's transcript export history (Admin/Faculty).</summary>
    [HttpGet("transcript/{studentProfileId:guid}/history")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetTranscriptHistory(Guid studentProfileId, CancellationToken ct)
    {
        var logs = await _service.GetExportHistoryAsync(studentProfileId, ct);
        return Ok(logs);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Extracts the authenticated user's ID from JWT claims.</summary>
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    /// <summary>
    /// Extracts the student profile ID from the "studentProfileId" JWT claim.
    /// Students must have this claim populated during login.
    /// </summary>
    private Guid GetCurrentStudentProfileId()
    {
        var claim = User.FindFirst("studentProfileId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
