using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Final-Touches Phase 19 Stage 19.4 — per-course grading configuration controller

/// <summary>
/// Manages per-course grading configurations.
/// SuperAdmin can create/update configs; Admin/Faculty/Student can read them.
/// </summary>
[ApiController]
[Route("api/v1/grading-config")]
[Authorize]
public class GradingConfigController : ControllerBase
{
    private readonly ICourseGradingService _service;

    public GradingConfigController(ICourseGradingService service) => _service = service;

    // Final-Touches Phase 19 Stage 19.4 — GET grading config for a course
    /// <summary>Returns the grading configuration for the specified course, or 404 if not set.</summary>
    [HttpGet("{courseId:guid}")]
    public async Task<IActionResult> Get(Guid courseId, CancellationToken ct)
    {
        var config = await _service.GetConfigAsync(courseId, ct);
        return config is null ? NotFound() : Ok(config);
    }

    // Final-Touches Phase 19 Stage 19.4 — PUT (upsert) grading config for a course
    /// <summary>Creates or updates the grading configuration for the specified course. SuperAdmin only.</summary>
    [HttpPut("{courseId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Upsert(Guid courseId, [FromBody] SaveCourseGradingConfigRequest request, CancellationToken ct)
    {
        try
        {
            var result = await _service.UpsertConfigAsync(courseId, request, ct);
            return Ok(result);
        }
        catch (InvalidOperationException ex) { return NotFound(ex.Message); }
        catch (ArgumentException ex)         { return BadRequest(ex.Message); }
    }
}
