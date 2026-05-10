using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Interfaces;

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
    private readonly IAuditService _audit;

    public InstitutionPolicyController(
        IInstitutionPolicyService policy,
        IAuditService audit)
    {
        _policy = policy;
        _audit = audit;
    }

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
            // Final-Touches Phase 31 Stage 31.2 — audit institution policy mutations.
            await _policy.SavePolicyAsync(new SaveInstitutionPolicyCommand(
                request.IncludeSchool,
                request.IncludeCollege,
                request.IncludeUniversity), ct);

            await _audit.LogAsync(new AuditLog(
                action: "InstitutionPolicySave",
                entityName: "InstitutionPolicy",
                actorUserId: GetUserId(),
                newValuesJson: $"{{\"includeSchool\":{request.IncludeSchool.ToString().ToLowerInvariant()},\"includeCollege\":{request.IncludeCollege.ToString().ToLowerInvariant()},\"includeUniversity\":{request.IncludeUniversity.ToString().ToLowerInvariant()}}}",
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()), ct);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
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
