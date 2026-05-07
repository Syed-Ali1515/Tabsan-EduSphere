// Final-Touches Phase 16 Stage 16.2 — Rubric controller

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Assignments;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// CRUD for rubrics and per-submission rubric grading.
/// Faculty: create/update/delete/grade.
/// Students/Others: read-only rubric and own grade views.
/// </summary>
[ApiController]
[Route("api/v1/rubric")]
[Authorize]
public class RubricController : ControllerBase
{
    // Final-Touches Phase 16 Stage 16.2 — service dependency
    private readonly IRubricService _service;

    public RubricController(IRubricService service) => _service = service;

    // ── Query ─────────────────────────────────────────────────────────────────

    /// <summary>Returns the active rubric for an assignment, or 404 if none exists.</summary>
    [HttpGet("assignment/{assignmentId:guid}")]
    public async Task<IActionResult> GetByAssignment(Guid assignmentId, CancellationToken ct)
    {
        var rubric = await _service.GetByAssignmentAsync(assignmentId, ct);
        return rubric is null ? NotFound() : Ok(rubric);
    }

    // ── Create ────────────────────────────────────────────────────────────────

    /// <summary>Creates a new rubric with criteria and levels.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> Create([FromBody] CreateRubricRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var id = await _service.CreateAsync(request, userId, ct);
            return Ok(new { rubricId = id });
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    // ── Update ────────────────────────────────────────────────────────────────

    /// <summary>Updates the rubric title.</summary>
    [HttpPut("{rubricId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> Update(Guid rubricId, [FromBody] UpdateRubricRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            await _service.UpdateAsync(rubricId, request, userId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
    }

    // ── Delete ────────────────────────────────────────────────────────────────

    /// <summary>Deactivates (soft-deletes) a rubric.</summary>
    [HttpDelete("{rubricId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> Delete(Guid rubricId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            await _service.DeleteAsync(rubricId, userId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Grade submission ──────────────────────────────────────────────────────

    /// <summary>
    /// Records rubric level selections for a student's submission.
    /// The caller must supply pre-resolved PointsAwarded values in the request
    /// (which come from the rubric the client already loaded).
    /// </summary>
    [HttpPost("{rubricId:guid}/grade")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GradeSubmission(
        Guid rubricId,
        [FromBody] RubricGradeRequest request,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            var response = await _service.GradeSubmissionAsync(request, userId, ct);
            return Ok(response);
        }
        catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    // ── Get grade ─────────────────────────────────────────────────────────────

    /// <summary>Returns the current rubric grade breakdown for a submission.</summary>
    [HttpGet("{rubricId:guid}/grade/{submissionId:guid}")]
    public async Task<IActionResult> GetSubmissionGrade(Guid rubricId, Guid submissionId, CancellationToken ct)
    {
        var response = await _service.GetSubmissionGradeAsync(rubricId, submissionId, ct);
        return response is null ? NotFound() : Ok(response);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
