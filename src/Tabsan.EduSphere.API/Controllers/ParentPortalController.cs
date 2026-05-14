using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Phase 26 — Stage 26.3

[ApiController]
[Route("api/v1/parent-portal")]
[Authorize]
public class ParentPortalController : ControllerBase
{
    private readonly IParentPortalService _service;

    public ParentPortalController(IParentPortalService service)
    {
        _service = service;
    }

    [HttpGet("me/students")]
    [Authorize(Roles = "Parent,Admin,SuperAdmin")]
    public async Task<IActionResult> GetMyLinkedStudents(CancellationToken ct)
    {
        var parentUserId = GetCurrentUserId();
        if (parentUserId == Guid.Empty)
            return BadRequest("Missing user_id claim.");

        var students = await _service.GetLinkedStudentsAsync(parentUserId, ct);
        return Ok(students);
    }

    [HttpGet("me/students/{studentProfileId:guid}/results")]
    [Authorize(Roles = "Parent,Admin,SuperAdmin")]
    public async Task<IActionResult> GetLinkedStudentResults(Guid studentProfileId, CancellationToken ct)
    {
        var parentUserId = GetCurrentUserId();
        if (parentUserId == Guid.Empty)
            return BadRequest("Missing user_id claim.");

        try
        {
            var items = await _service.GetLinkedStudentResultsAsync(parentUserId, studentProfileId, ct);
            return Ok(items);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("me/students/{studentProfileId:guid}/attendance")]
    [Authorize(Roles = "Parent,Admin,SuperAdmin")]
    public async Task<IActionResult> GetLinkedStudentAttendance(Guid studentProfileId, [FromQuery] Guid? courseOfferingId, CancellationToken ct)
    {
        var parentUserId = GetCurrentUserId();
        if (parentUserId == Guid.Empty)
            return BadRequest("Missing user_id claim.");

        try
        {
            var items = await _service.GetLinkedStudentAttendanceAsync(parentUserId, studentProfileId, courseOfferingId, ct);
            return Ok(items);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("me/students/{studentProfileId:guid}/announcements")]
    [Authorize(Roles = "Parent,Admin,SuperAdmin")]
    public async Task<IActionResult> GetLinkedStudentAnnouncements(Guid studentProfileId, [FromQuery] Guid? courseOfferingId, CancellationToken ct)
    {
        var parentUserId = GetCurrentUserId();
        if (parentUserId == Guid.Empty)
            return BadRequest("Missing user_id claim.");

        try
        {
            var items = await _service.GetLinkedStudentAnnouncementsAsync(parentUserId, studentProfileId, courseOfferingId, ct);
            return Ok(items);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("me/students/{studentProfileId:guid}/timetable")]
    [Authorize(Roles = "Parent,Admin,SuperAdmin")]
    public async Task<IActionResult> GetLinkedStudentTimetable(Guid studentProfileId, [FromQuery] Guid? timetableId, CancellationToken ct)
    {
        var parentUserId = GetCurrentUserId();
        if (parentUserId == Guid.Empty)
            return BadRequest("Missing user_id claim.");

        try
        {
            var item = await _service.GetLinkedStudentTimetableAsync(parentUserId, studentProfileId, timetableId, ct);
            return Ok(item);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("links/{parentUserId:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetLinksByParent(Guid parentUserId, CancellationToken ct)
        => Ok(await _service.GetLinksByParentAsync(parentUserId, ct));

    [HttpPut("links")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpsertLink([FromBody] UpsertParentStudentLinkRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await _service.UpsertLinkAsync(request, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("links/{parentUserId:guid}/{studentProfileId:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> DeactivateLink(Guid parentUserId, Guid studentProfileId, CancellationToken ct)
    {
        var changed = await _service.DeactivateLinkAsync(parentUserId, studentProfileId, ct);
        return changed ? NoContent() : NotFound();
    }

    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirst("user_id")?.Value
                  ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
