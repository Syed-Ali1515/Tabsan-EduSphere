using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Final-Touches Phase 30 Stage 30.3 — feature flag rollout and rollback controller.
[ApiController]
[Route("api/v1/feature-flags")]
[Authorize(Roles = "SuperAdmin")]
public sealed class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;

    public FeatureFlagsController(IFeatureFlagService featureFlags)
    {
        _featureFlags = featureFlags;
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
        await _featureFlags.SaveAsync(command with { Key = key }, ct);
        return NoContent();
    }

    [HttpPost("rollback")]
    public async Task<IActionResult> Rollback(
        [FromBody] RollbackFeatureFlagsCommand command,
        CancellationToken ct)
    {
        await _featureFlags.RollbackAsync(command, ct);
        return NoContent();
    }
}
