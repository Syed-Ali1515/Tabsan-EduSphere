using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages admin change requests submitted by students and teachers.
/// Students and teachers can create/cancel requests.
/// Admins can view pending and approve/reject.
/// Routes: /api/v1/change-requests
/// </summary>
[ApiController]
[Route("api/v1/change-requests")]
[Authorize]
public class AdminChangeRequestController : ControllerBase
{
    private readonly IStudentLifecycleService _service;

    public AdminChangeRequestController(IStudentLifecycleService service)
    {
        _service = service;
    }

    private Guid GetUserId()
    {
        var val = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(val, out var id) ? id : Guid.Empty;
    }

    // ── POST /api/v1/change-requests ──────────────────────────────────────────

    /// <summary>Student or teacher submits a change request for profile modification.</summary>
    [HttpPost]
    [Authorize(Roles = "Student,Faculty")]
    public async Task<IActionResult> Create([FromBody] CreateChangeRequestCommand cmd, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        var result = await _service.CreateChangeRequestAsync(userId, cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // ── GET /api/v1/change-requests/pending ───────────────────────────────────

    /// <summary>Returns all pending change requests. Admin only.</summary>
    [HttpGet("pending")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetPending(CancellationToken ct)
    {
        var requests = await _service.GetPendingChangeRequestsAsync(ct);
        return Ok(requests);
    }

    // ── GET /api/v1/change-requests/my ────────────────────────────────────────

    /// <summary>Returns all change requests for the currently authenticated user.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        var requests = await _service.GetChangeRequestsByUserAsync(userId, ct);
        return Ok(requests);
    }

    // ── GET /api/v1/change-requests/{id} ─────────────────────────────────────

    /// <summary>Returns a specific change request by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var request = await _service.GetChangeRequestByIdAsync(id, ct);
        if (request is null) return NotFound();
        return Ok(request);
    }

    // ── POST /api/v1/change-requests/{id}/approve ────────────────────────────

    /// <summary>Admin approves a change request.</summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] string? notes, CancellationToken ct)
    {
        var adminId = GetUserId();
        if (adminId == Guid.Empty) return Forbid();

        try
        {
            await _service.ApproveChangeRequestAsync(id, adminId, notes, ct);
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

    // ── POST /api/v1/change-requests/{id}/reject ─────────────────────────────

    /// <summary>Admin rejects a change request.</summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] string? notes, CancellationToken ct)
    {
        var adminId = GetUserId();
        if (adminId == Guid.Empty) return Forbid();

        try
        {
            await _service.RejectChangeRequestAsync(id, adminId, notes, ct);
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
