using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> GetMyLinkedStudents(CancellationToken ct)
    {
        var parentUserIdClaim = User.FindFirst("user_id")?.Value;
        if (!Guid.TryParse(parentUserIdClaim, out var parentUserId))
            return BadRequest("Missing user_id claim.");

        var students = await _service.GetLinkedStudentsAsync(parentUserId, ct);
        return Ok(students);
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
}
