using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages the room catalogue within buildings.
/// GET endpoints are available to all authenticated users (dropdown population).
/// Write endpoints are restricted to Admin and SuperAdmin.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class RoomController : ControllerBase
{
    private readonly IBuildingRoomService _service;

    public RoomController(IBuildingRoomService service) => _service = service;

    // ── GET /api/v1/room ──────────────────────────────────────────────────────

    /// <summary>Returns all active rooms across all buildings (for dropdown population).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var list = await _service.GetAllRoomsAsync(activeOnly, ct);
        return Ok(list);
    }

    // ── GET /api/v1/room/building/{buildingId} ────────────────────────────────

    /// <summary>Returns all active rooms within a specific building.</summary>
    [HttpGet("building/{buildingId:guid}")]
    public async Task<IActionResult> GetByBuilding(Guid buildingId, [FromQuery] bool activeOnly = true, CancellationToken ct = default)
    {
        var list = await _service.GetRoomsByBuildingAsync(buildingId, activeOnly, ct);
        return Ok(list);
    }

    // ── GET /api/v1/room/{id} ─────────────────────────────────────────────────

    /// <summary>Returns a single room by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var dto = await _service.GetRoomByIdAsync(id, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/room ─────────────────────────────────────────────────────

    /// <summary>Creates a new room within a building.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateRoomCommand cmd, CancellationToken ct)
    {
        try
        {
            var dto = await _service.CreateRoomAsync(cmd, ct);
            return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── PUT /api/v1/room/{id} ─────────────────────────────────────────────────

    /// <summary>Updates a room's number and capacity.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoomCommand cmd, CancellationToken ct)
    {
        try
        {
            var dto = await _service.UpdateRoomAsync(id, cmd, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/room/{id}/activate ───────────────────────────────────────

    /// <summary>Activates a room so it appears in timetable dropdowns.</summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.ActivateRoomAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/room/{id}/deactivate ─────────────────────────────────────

    /// <summary>Deactivates a room so it is hidden from timetable dropdowns.</summary>
    [HttpPost("{id:guid}/deactivate")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeactivateRoomAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
