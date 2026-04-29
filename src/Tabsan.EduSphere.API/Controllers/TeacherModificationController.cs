using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages teacher requests to modify attendance or result records.
/// Teachers create modification requests; admins review and approve/reject.
/// All requests include a mandatory reason and a full audit trail.
/// Routes: /api/v1/modification-requests
/// </summary>
[ApiController]
[Route("api/v1/modification-requests")]
[Authorize]
public class TeacherModificationController : ControllerBase
{
    private readonly IStudentLifecycleService _service;

    public TeacherModificationController(IStudentLifecycleService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var val = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(val, out var id) ? id : Guid.Empty;
    }

    // ── POST /api/v1/modification-requests ───────────────────────────────────

    /// <summary>Teacher creates a modification request for an attendance or result record.</summary>
    [HttpPost]
    [Authorize(Roles = "Faculty")]
    public async Task<IActionResult> Create([FromBody] CreateModificationRequestCommand cmd, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        try
        {
            var result = await _service.CreateModificationRequestAsync(userId, cmd, ct);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    // ── GET /api/v1/modification-requests/pending ─────────────────────────────

    /// <summary>Returns all pending modification requests. Admin only.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var requests = await _service.GetPendingModificationRequestsAsync(ct);
        return Ok(requests);
    }

    // ── GET /api/v1/modification-requests/my ─────────────────────────────────

    /// <summary>Returns all modification requests for the currently authenticated teacher.</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Faculty")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        var requests = await _service.GetModificationRequestsByTeacherAsync(userId, ct);
        return Ok(requests);
    }

    // ── GET /api/v1/modification-requests/{id} ────────────────────────────────

    /// <summary>Returns a specific modification request by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var request = await _service.GetModificationRequestByIdAsync(id, ct);
        if (request is null) return NotFound();
        return Ok(request);
    }

    // ── POST /api/v1/modification-requests/{id}/approve ──────────────────────

    /// <summary>Admin approves a modification request. Admin must apply the actual record change afterward.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] string? notes, CancellationToken ct)
    {
        var adminId = GetUserId();
        if (adminId == Guid.Empty) return Forbid();

        try
        {
            await _service.ApproveModificationRequestAsync(id, adminId, notes, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    // ── POST /api/v1/modification-requests/{id}/reject ────────────────────────

    /// <summary>Admin rejects a modification request. Record remains unchanged.</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string? notes, CancellationToken ct)
    {
        var adminId = GetUserId();
        if (adminId == Guid.Empty) return Forbid();

        try
        {
            await _service.RejectModificationRequestAsync(id, adminId, notes, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }
}
