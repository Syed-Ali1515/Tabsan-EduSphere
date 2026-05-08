using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Phase 23 Stage 23.1/23.2 — Institution Policy API.
/// GET  api/v1/institution-policy  — readable by all authenticated roles.
/// PUT  api/v1/institution-policy  — SuperAdmin only.
/// </summary>
[ApiController]
[Route("api/v1/institution-policy")]
[Authorize]
public sealed class InstitutionPolicyController : ControllerBase
{
    private readonly IInstitutionPolicyService _policy;

    public InstitutionPolicyController(IInstitutionPolicyService policy)
        => _policy = policy;

    // ── GET ───────────────────────────────────────────────────────────────────

    /// <summary>Returns the current institution policy flags.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var snapshot = await _policy.GetPolicyAsync(ct);
        return Ok(new InstitutionPolicyResponse(
            snapshot.IncludeSchool,
            snapshot.IncludeCollege,
            snapshot.IncludeUniversity,
            snapshot.IsValid));
    }

    // ── PUT ───────────────────────────────────────────────────────────────────

    /// <summary>Saves institution type flags. SuperAdmin only.</summary>
    [HttpPut]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Save(
        [FromBody] SaveInstitutionPolicyRequest request,
        CancellationToken ct)
    {
        try
        {
            await _policy.SavePolicyAsync(new SaveInstitutionPolicyCommand(
                request.IncludeSchool,
                request.IncludeCollege,
                request.IncludeUniversity), ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// ── Request / Response models ─────────────────────────────────────────────────

public sealed record InstitutionPolicyResponse(
    bool IncludeSchool,
    bool IncludeCollege,
    bool IncludeUniversity,
    bool IsValid);

public sealed record SaveInstitutionPolicyRequest(
    bool IncludeSchool,
    bool IncludeCollege,
    bool IncludeUniversity);
