using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;
using System.Security.Claims;

namespace Tabsan.EduSphere.API.Controllers;

// Phase 25 — Academic Engine Unification — Stage 25.3

/// <summary>
/// Evaluates and applies student academic progression decisions.
/// Admin can evaluate; Admin can also promote. Students may view their own evaluation.
/// </summary>
[ApiController]
[Route("api/v1/progression")]
[Authorize]
public class ProgressionController : ControllerBase
{
    private readonly IProgressionService _service;

    public ProgressionController(IProgressionService service)
    {
        _service = service;
    }

    // ── POST /api/v1/progression/evaluate ─────────────────────────────────────

    /// <summary>
    /// Evaluates whether a student can progress to the next period.
    /// Returns the decision without making any changes. Requires Admin or higher.
    /// </summary>
    [HttpPost("evaluate")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Evaluate(
        [FromBody] ProgressionEvaluationRequest request,
        CancellationToken ct)
    {
        try
        {
            var decision = await _service.EvaluateAsync(request, ct);
            return Ok(decision);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/progression/promote ──────────────────────────────────────

    /// <summary>
    /// Promotes a student to the next academic period if they meet the criteria.
    /// Requires Admin or higher. Throws 400 when the student does not qualify.
    /// </summary>
    [HttpPost("promote")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Promote(
        [FromBody] ProgressionEvaluationRequest request,
        CancellationToken ct)
    {
        try
        {
            var decision = await _service.PromoteAsync(request, ct);
            return Ok(decision);
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

    // ── GET /api/v1/progression/me/{type} ─────────────────────────────────────

    /// <summary>
    /// Allows the authenticated student to view their own progression eligibility.
    /// Requires Student role or higher.
    /// </summary>
    [HttpGet("me/{type}")]
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> GetMyProgression(InstitutionType type, CancellationToken ct)
    {
        var studentProfileIdClaim = User.FindFirstValue("studentProfileId")
            ?? User.FindFirstValue("student_profile_id");
        if (!Guid.TryParse(studentProfileIdClaim, out var profileId))
            return Forbid();

        try
        {
            var decision = await _service.EvaluateAsync(
                new ProgressionEvaluationRequest(profileId, type), ct);
            return Ok(decision);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
