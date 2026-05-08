using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.API.Controllers;

// Phase 25 — Academic Engine Unification — Stage 25.2

/// <summary>
/// Manages institution-level grading profiles (pass threshold + grade bands).
/// SuperAdmin: full create / update access.
/// Admin: read-only.
/// </summary>
[ApiController]
[Route("api/v1/institution-grading-profiles")]
[Authorize]
public class InstitutionGradingProfileController : ControllerBase
{
    private readonly IInstitutionGradingService _service;

    public InstitutionGradingProfileController(IInstitutionGradingService service)
    {
        _service = service;
    }

    // ── GET /api/v1/institution-grading-profiles ──────────────────────────────

    /// <summary>Returns all institution grading profiles. Requires Admin or higher.</summary>
    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var profiles = await _service.GetAllAsync(ct);
        return Ok(profiles);
    }

    // ── GET /api/v1/institution-grading-profiles/{type} ───────────────────────

    /// <summary>Returns the grading profile for the given institution type, or 404. Requires Admin or higher.</summary>
    [HttpGet("{type}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetByType(InstitutionType type, CancellationToken ct)
    {
        var profile = await _service.GetByTypeAsync(type, ct);
        if (profile is null)
            return NotFound(new { message = $"No grading profile found for institution type '{type}'." });
        return Ok(profile);
    }

    // ── PUT /api/v1/institution-grading-profiles/{type} ───────────────────────

    /// <summary>
    /// Creates or updates the grading profile for the given institution type.
    /// Requires SuperAdmin role.
    /// </summary>
    [HttpPut("{type}")]
    [Authorize(Policy = "SuperAdmin")]
    public async Task<IActionResult> Upsert(
        InstitutionType type,
        [FromBody] SaveInstitutionGradingProfileRequest request,
        CancellationToken ct)
    {
        try
        {
            var dto = await _service.UpsertAsync(type, request, ct);
            return Ok(dto);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
