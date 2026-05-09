using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Phase 26 — Stage 26.1

[ApiController]
[Route("api/v1/school-streams")]
[Authorize]
public class SchoolStreamController : ControllerBase
{
    private readonly ISchoolStreamService _service;

    public SchoolStreamController(ISchoolStreamService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _service.GetAllStreamsAsync(ct));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Upsert(Guid id, [FromBody] SaveSchoolStreamRequest request, CancellationToken ct)
        => Ok(await _service.UpsertStreamAsync(id == Guid.Empty ? null : id, request, ct));

    [HttpPost("assign")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Assign([FromBody] AssignStudentStreamRequest request, CancellationToken ct)
        => Ok(await _service.AssignStudentAsync(request, ct));

    [HttpGet("student/{studentProfileId:guid}")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetStudentAssignment(Guid studentProfileId, CancellationToken ct)
    {
        var assignment = await _service.GetStudentAssignmentAsync(studentProfileId, ct);
        return assignment is null ? NotFound() : Ok(assignment);
    }
}
