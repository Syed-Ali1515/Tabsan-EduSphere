using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Final-Touches Phase 30 Stage 30.3 — feature flag rollout and rollback controller.
[ApiController]
[Route("api/v1/feature-flags")]
[Authorize(Roles = "SuperAdmin")]
public sealed class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;
    private readonly IAuditService _audit;

    public FeatureFlagsController(
        IFeatureFlagService featureFlags,
        IAuditService audit)
    {
        _featureFlags = featureFlags;
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
        => Ok(await _featureFlags.GetAllAsync(ct));

    [HttpGet("{key}")]
    public async Task<IActionResult> GetByKey([FromRoute] string key, CancellationToken ct)
        => Ok(await _featureFlags.GetAsync(key, ct));

    [HttpPut("{key}")]
    public async Task<IActionResult> Save(
        [FromRoute] string key,
        [FromBody] SaveFeatureFlagCommand command,
        CancellationToken ct)
    {
        // Final-Touches Phase 31 Stage 31.2 — audit sensitive feature-flag mutations.
        await _featureFlags.SaveAsync(command with { Key = key }, ct);

        await _audit.LogAsync(new AuditLog(
            action: "FeatureFlagSave",
            entityName: "FeatureFlag",
            entityId: key,
            actorUserId: GetUserId(),
            newValuesJson: $"{{\"key\":\"{key}\",\"isEnabled\":{command.IsEnabled.ToString().ToLowerInvariant()}}}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()), ct);

        return NoContent();
    }

    [HttpPost("rollback")]
    public async Task<IActionResult> Rollback(
        [FromBody] RollbackFeatureFlagsCommand command,
        CancellationToken ct)
    {
        // Final-Touches Phase 31 Stage 31.2 — audit rollback operations for incident traceability.
        await _featureFlags.RollbackAsync(command, ct);

        await _audit.LogAsync(new AuditLog(
            action: "FeatureFlagRollback",
            entityName: "FeatureFlag",
            entityId: string.Join(",", command.Keys ?? []),
            actorUserId: GetUserId(),
            newValuesJson: $"{{\"reason\":\"{command.Reason ?? string.Empty}\"}}",
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()), ct);

        return NoContent();
    }

    private Guid GetUserId()
    {
        var raw = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
