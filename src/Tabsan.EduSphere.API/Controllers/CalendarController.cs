using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// REST API for the Academic Calendar feature (Phase 12).
/// GET endpoints are open to all authenticated users.
/// Write endpoints require Admin or SuperAdmin.
/// </summary>
[ApiController]
[Route("api/v1/calendar")]
[Authorize]
public class CalendarController : ControllerBase
{
    private readonly IAcademicCalendarService _calendar;

    public CalendarController(IAcademicCalendarService calendar) => _calendar = calendar;

    // ── Read endpoints (all authenticated roles) ──────────────────────────────

    /// <summary>Returns all active deadlines across all semesters ordered by date.</summary>
    [HttpGet("deadlines")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var items = await _calendar.GetAllAsync(ct);
        return Ok(items);
    }

    /// <summary>Returns active deadlines for a specific semester.</summary>
    [HttpGet("deadlines/by-semester/{semesterId:guid}")]
    public async Task<IActionResult> GetBySemester(Guid semesterId, CancellationToken ct)
    {
        var items = await _calendar.GetBySemesterAsync(semesterId, ct);
        return Ok(items);
    }

    /// <summary>Returns the detail of a single deadline by ID.</summary>
    [HttpGet("deadlines/{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var item = await _calendar.GetByIdAsync(id, ct);
        if (item is null) return NotFound();
        return Ok(item);
    }

    // ── Write endpoints (Admin / SuperAdmin only) ─────────────────────────────

    /// <summary>Creates a new academic deadline.</summary>
    [HttpPost("deadlines")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDeadlineRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required.");

        var created = await _calendar.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing academic deadline.</summary>
    [HttpPut("deadlines/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDeadlineRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required.");

        var updated = await _calendar.UpdateAsync(id, request, ct);
        if (updated is null) return NotFound();
        return Ok(updated);
    }

    /// <summary>Soft-deletes an academic deadline.</summary>
    [HttpDelete("deadlines/{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var deleted = await _calendar.DeleteAsync(id, ct);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
