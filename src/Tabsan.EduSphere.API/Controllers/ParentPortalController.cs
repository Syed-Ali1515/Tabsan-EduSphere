using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
}
