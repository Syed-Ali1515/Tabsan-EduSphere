using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages system report definitions and their role assignments.
/// Only Super Admin can create, modify, or deactivate report definitions.
/// </summary>
[ApiController]
[Route("api/v1/report-settings")]
[Authorize(Roles = "SuperAdmin")]
public class ReportSettingsController : ControllerBase
{
    private readonly IReportSettingsService _service;

    public ReportSettingsController(IReportSettingsService service) => _service = service;

    // ── GET /api/v1/report-settings ───────────────────────────────────────────

    /// <summary>Returns all report definitions with their current role assignments.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var reports = await _service.GetAllAsync(ct);
        return Ok(reports);
    }

    // ── GET /api/v1/report-settings/{key} ─────────────────────────────────────

    /// <summary>Returns a single report definition by its stable key.</summary>
    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey(string key, CancellationToken ct)
    {
        try
        {
            var dto = await _service.GetByKeyAsync(key, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/report-settings ──────────────────────────────────────────

    /// <summary>Creates a new report definition.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportCommand cmd, CancellationToken ct)
    {
        try
        {
            var dto = await _service.CreateAsync(cmd, ct);
            return CreatedAtAction(nameof(GetByKey), new { key = dto.Key }, dto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // ── PUT /api/v1/report-settings/{id} ──────────────────────────────────────

    /// <summary>Updates the name and purpose of a report definition.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReportCommand cmd, CancellationToken ct)
    {
        try
        {
            var dto = await _service.UpdateAsync(id, cmd, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/report-settings/{id}/activate ───────────────────────────

    /// <summary>Activates the report so it appears on role dashboards.</summary>
    [HttpPost("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.ActivateAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/report-settings/{id}/deactivate ─────────────────────────

    /// <summary>Deactivates the report (hidden from role dashboards; Super Admin still sees it).</summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeactivateAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── PUT /api/v1/report-settings/{id}/roles ────────────────────────────────

    /// <summary>Replaces all role assignments for a report. Pass an empty array to clear all.</summary>
    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] SetRolesCommand cmd, CancellationToken ct)
    {
        try
        {
            await _service.SetRolesAsync(id, cmd, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
