using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages the building catalogue used as timetable entry dropdown source.
/// GET endpoints are available to all authenticated users (dropdown population).
/// Write endpoints are restricted to Admin and SuperAdmin.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BuildingController : ControllerBase
{
    private readonly IBuildingRoomService _service;

    public BuildingController(IBuildingRoomService service) => _service = service;

    // ── GET /api/v1/building ──────────────────────────────────────────────────

    /// <summary>Returns all active buildings (for dropdown population).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var list = await _service.GetAllBuildingsAsync(activeOnly, ct);
        return Ok(list);
    }

    // ── GET /api/v1/building/{id} ─────────────────────────────────────────────

    /// <summary>Returns a single building by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var dto = await _service.GetBuildingByIdAsync(id, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/building ─────────────────────────────────────────────────

    /// <summary>Creates a new building.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateBuildingCommand cmd, CancellationToken ct)
    {
        var dto = await _service.CreateBuildingAsync(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // ── PUT /api/v1/building/{id} ─────────────────────────────────────────────

    /// <summary>Updates a building's name and code.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBuildingCommand cmd, CancellationToken ct)
    {
        try
        {
            var dto = await _service.UpdateBuildingAsync(id, cmd, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/building/{id}/activate ──────────────────────────────────

    /// <summary>Activates a building so it appears in timetable dropdowns.</summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.ActivateBuildingAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/building/{id}/deactivate ─────────────────────────────────

    /// <summary>Deactivates a building so it is hidden from timetable dropdowns.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeactivateBuildingAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
