using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Final-Touches Phase 30 Stage 30.2 — tenant onboarding, subscription, and profile operations.
[ApiController]
[Route("api/v1/tenant-operations")]
[Authorize(Roles = "SuperAdmin")]
public sealed class TenantOperationsController : ControllerBase
{
    private readonly ITenantOperationsService _service;

    public TenantOperationsController(ITenantOperationsService service)
    {
        _service = service;
    }

    [HttpGet("onboarding-template")]
    public async Task<IActionResult> GetOnboardingTemplate(CancellationToken ct)
        => Ok(await _service.GetOnboardingTemplateAsync(ct));

    [HttpPut("onboarding-template")]
    public async Task<IActionResult> SaveOnboardingTemplate(
        [FromBody] SaveTenantOnboardingTemplateCommand command,
        CancellationToken ct)
    {
        await _service.SaveOnboardingTemplateAsync(command, ct);
        return NoContent();
    }

    [HttpGet("subscription-plan")]
    public async Task<IActionResult> GetSubscriptionPlan(CancellationToken ct)
        => Ok(await _service.GetSubscriptionPlanAsync(ct));

    [HttpPut("subscription-plan")]
    public async Task<IActionResult> SaveSubscriptionPlan(
        [FromBody] SaveTenantSubscriptionPlanCommand command,
        CancellationToken ct)
    {
        await _service.SaveSubscriptionPlanAsync(command, ct);
        return NoContent();
    }

    [HttpGet("tenant-profile")]
    public async Task<IActionResult> GetTenantProfile(CancellationToken ct)
        => Ok(await _service.GetTenantProfileAsync(ct));

    [HttpPut("tenant-profile")]
    public async Task<IActionResult> SaveTenantProfile(
        [FromBody] SaveTenantProfileSettingsCommand command,
        CancellationToken ct)
    {
        await _service.SaveTenantProfileAsync(command, ct);
        return NoContent();
    }
}
