// Final-Touches Phase 16 Stage 16.1/16.3 — Gradebook controller

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Assignments;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Provides the gradebook grid, inline result upsert, publish-all,
/// and CSV bulk-grading (template / preview / confirm) endpoints.
/// Faculty: full write access for their own offerings.
/// Admin/SuperAdmin: read + publish access.
/// </summary>
[ApiController]
[Route("api/v1/gradebook")]
[Authorize]
public class GradebookController : ControllerBase
{
    // Final-Touches Phase 16 Stage 16.1 — service dependency
    private readonly IGradebookService _service;

    public GradebookController(IGradebookService service) => _service = service;

    // ── Stage 16.1: Grid ──────────────────────────────────────────────────────

    /// <summary>Returns the complete gradebook grid for a course offering.</summary>
    [HttpGet("{offeringId:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetGrid(Guid offeringId, CancellationToken ct)
    {
        try
        {
            var grid = await _service.GetGradebookAsync(offeringId, ct);
            return Ok(grid);
        }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    // ── Stage 16.1: Upsert single entry ──────────────────────────────────────

    /// <summary>Creates or corrects a single result cell in the gradebook.</summary>
    [HttpPut("{offeringId:guid}/entry")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> UpsertEntry(Guid offeringId, [FromBody] UpsertGradebookEntryRequest request, CancellationToken ct)
    {
        if (request.CourseOfferingId != offeringId)
            return BadRequest("offeringId in route and request body must match.");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            await _service.UpsertEntryAsync(request, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    // ── Stage 16.1: Publish all ───────────────────────────────────────────────

    /// <summary>Publishes all draft results for the offering.</summary>
    [HttpPost("{offeringId:guid}/publish-all")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> PublishAll(Guid offeringId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await _service.PublishAllAsync(offeringId, userId, ct);
        return NoContent();
    }

    // ── Stage 16.3: CSV template ──────────────────────────────────────────────

    /// <summary>Downloads a pre-populated CSV template for bulk grading a component.</summary>
    [HttpGet("{offeringId:guid}/template")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetTemplate(Guid offeringId, [FromQuery] string component, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(component))
            return BadRequest("component query parameter is required.");

        var bytes = await _service.GetCsvTemplateAsync(offeringId, component, ct);
        return File(bytes, "text/csv", $"gradebook-{component}-template.csv");
    }

    // ── Stage 16.3: CSV upload preview ────────────────────────────────────────

    /// <summary>Parses an uploaded CSV and returns a validation preview (does not save).</summary>
    [HttpPost("{offeringId:guid}/bulk-grade")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> BulkGradePreview(
        Guid offeringId,
        [FromQuery] string component,
        IFormFile file,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(component))
            return BadRequest("component query parameter is required.");
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        await using var stream = file.OpenReadStream();
        var preview = await _service.ParseBulkCsvAsync(offeringId, component, stream, ct);
        return Ok(preview);
    }

    // ── Stage 16.3: CSV confirm ───────────────────────────────────────────────

    /// <summary>Applies the previously validated bulk-grade rows.</summary>
    [HttpPost("{offeringId:guid}/bulk-grade/confirm")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> BulkGradeConfirm(
        Guid offeringId,
        [FromBody] BulkGradeConfirmRequest request,
        CancellationToken ct)
    {
        if (request.CourseOfferingId != offeringId)
            return BadRequest("offeringId in route and request body must match.");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        try
        {
            await _service.ConfirmBulkGradeAsync(request, userId, ct);
            return NoContent();
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (Exception ex) { return StatusCode(500, ex.Message); }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
