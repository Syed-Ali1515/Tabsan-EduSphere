using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tabsan.EduSphere.Application.DTOs.Helpdesk;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// REST API for the Helpdesk / Support Ticketing feature (Phase 14).
/// Ticket submission open to all authenticated roles (Student, Faculty, Admin, SuperAdmin).
/// Case management restricted to Admin / SuperAdmin; Faculty can manage tickets assigned to them.
/// </summary>
[ApiController]
[Route("api/v1/helpdesk")]
[Authorize]
public class HelpdeskController : ControllerBase
{
    private readonly IHelpdeskService            _helpdesk;
    private readonly IAdminAssignmentRepository  _adminAssignments;

    public HelpdeskController(IHelpdeskService helpdesk, IAdminAssignmentRepository adminAssignments)
    {
        _helpdesk         = helpdesk;
        _adminAssignments = adminAssignments;
    }

    // ── Ticket listing ────────────────────────────────────────────────────────

    /// <summary>Returns ticket list scoped to the caller's role.</summary>
    [HttpGet("tickets")]
    public async Task<IActionResult> GetTickets([FromQuery] TicketStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var (callerId, callerRole) = ExtractCaller();
        if (callerId == Guid.Empty) return Unauthorized();

        IReadOnlyList<Guid>? deptIds = null;
        if (callerRole == "Admin")
        {
            var ids = await _adminAssignments.GetDepartmentIdsForAdminAsync(callerId, ct);
            deptIds = ids.ToList();
        }

        var tickets = await _helpdesk.GetTicketsAsync(callerId, callerRole, deptIds, status, page, pageSize, ct);
        return Ok(tickets);
    }

    // ── Ticket detail ─────────────────────────────────────────────────────────

    [HttpGet("tickets/{id:guid}")]
    public async Task<IActionResult> GetTicket(Guid id, CancellationToken ct)
    {
        var (callerId, callerRole) = ExtractCaller();
        if (callerId == Guid.Empty) return Unauthorized();

        var detail = await _helpdesk.GetTicketDetailAsync(id, callerId, callerRole, ct);
        if (detail is null) return NotFound();
        return Ok(detail);
    }

    // ── Create ticket (all authenticated roles) ───────────────────────────────

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketApiRequest body, CancellationToken ct)
    {
        var (callerId, _) = ExtractCaller();
        if (callerId == Guid.Empty) return Unauthorized();

        var request = new CreateTicketRequest(
            callerId,
            body.DepartmentId,
            body.Category,
            body.Subject,
            body.Body);

        var id = await _helpdesk.CreateTicketAsync(request, ct);
        return CreatedAtAction(nameof(GetTicket), new { id }, new { id });
    }

    // ── Add message (all authenticated roles) ─────────────────────────────────

    [HttpPost("tickets/{id:guid}/messages")]
    public async Task<IActionResult> AddMessage(Guid id, [FromBody] AddMessageApiRequest body, CancellationToken ct)
    {
        var (callerId, callerRole) = ExtractCaller();
        if (callerId == Guid.Empty) return Unauthorized();

        // Only staff may post internal notes
        bool allowInternal = callerRole is "SuperAdmin" or "Admin" or "Faculty";
        bool isInternal    = body.IsInternalNote && allowInternal;

        var request = new AddMessageRequest(id, callerId, body.Body, isInternal);
        var msgId   = await _helpdesk.AddMessageAsync(request, ct);
        return Ok(new { id = msgId });
    }

    // ── Assign ticket (Admin / SuperAdmin) ────────────────────────────────────

    [HttpPut("tickets/{id:guid}/assign")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AssignTicket(Guid id, [FromBody] AssignTicketApiRequest body, CancellationToken ct)
    {
        await _helpdesk.AssignTicketAsync(new AssignTicketRequest(id, body.AssignedToId), ct);
        return NoContent();
    }

    // ── Resolve ticket ────────────────────────────────────────────────────────

    [HttpPut("tickets/{id:guid}/resolve")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ResolveTicket(Guid id, CancellationToken ct)
    {
        var (callerId, callerRole) = ExtractCaller();
        if (callerId == Guid.Empty) return Unauthorized();

        try
        {
            await _helpdesk.ResolveTicketAsync(id, callerId, callerRole, ct);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return NoContent();
    }

    // ── Close ticket (Admin / SuperAdmin) ─────────────────────────────────────

    [HttpPut("tickets/{id:guid}/close")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CloseTicket(Guid id, CancellationToken ct)
    {
        var (callerId, callerRole) = ExtractCaller();
        if (callerId == Guid.Empty) return Unauthorized();

        try
        {
            await _helpdesk.CloseTicketAsync(id, callerId, callerRole, ct);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return NoContent();
    }

    // ── Re-open ticket (submitter only) ───────────────────────────────────────

    [HttpPut("tickets/{id:guid}/reopen")]
    public async Task<IActionResult> ReopenTicket(Guid id, CancellationToken ct)
    {
        var (callerId, _) = ExtractCaller();
        if (callerId == Guid.Empty) return Unauthorized();

        try
        {
            await _helpdesk.ReopenTicketAsync(id, callerId, ct);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return NoContent();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private (Guid id, string role) ExtractCaller()
    {
        var idStr  = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        var role   = User.FindFirstValue(ClaimTypes.Role) ?? "";
        return Guid.TryParse(idStr, out var id) ? (id, role) : (Guid.Empty, "");
    }
}

// ── API request shapes (not DTOs — only used at HTTP boundary) ────────────────

public record CreateTicketApiRequest(
    Guid?          DepartmentId,
    TicketCategory Category,
    string         Subject,
    string         Body
);

public record AddMessageApiRequest(
    string Body,
    bool   IsInternalNote = false
);

public record AssignTicketApiRequest(Guid AssignedToId);
