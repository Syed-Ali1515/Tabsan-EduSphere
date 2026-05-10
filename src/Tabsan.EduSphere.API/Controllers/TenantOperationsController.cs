using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Final-Touches Phase 30 Stage 30.2 — tenant onboarding, subscription, and profile operations.
[ApiController]
[Route("api/v1/tenant-operations")]
[Authorize(Roles = "SuperAdmin")]
public sealed class TenantOperationsController : ControllerBase
{
    private readonly ITenantOperationsService _service;
    private readonly IFeatureFlagService _flags;
    private readonly IAuditService _audit;

    public TenantOperationsController(
        ITenantOperationsService service,
        IFeatureFlagService flags,
        IAuditService audit)
    {
        _service = service;
        _flags = flags;
        _audit = audit;
    }

    [HttpGet("onboarding-template")]
    public async Task<IActionResult> GetOnboardingTemplate(CancellationToken ct)
        => Ok(await _service.GetOnboardingTemplateAsync(ct));

    [HttpPut("onboarding-template")]
    public async Task<IActionResult> SaveOnboardingTemplate(
        [FromBody] SaveTenantOnboardingTemplateCommand command,
        CancellationToken ct)
    {
        var enabled = await _flags.GetAsync("tenant-operations.write", ct);
        if (!enabled.IsEnabled)
        {
            // Final-Touches Phase 31 Stage 31.2 — audit blocked control-plane writes.
            await _audit.LogAsync(new AuditLog(
                action: "TenantOperationsWriteBlocked",
                entityName: "TenantOnboardingTemplate",
                actorUserId: GetUserId(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()), ct);

            return StatusCode(StatusCodes.Status423Locked, new { message = "Tenant operations write path is disabled by feature flag for rollback safety." });
        }

        await _service.SaveOnboardingTemplateAsync(command, ct);

        await _audit.LogAsync(new AuditLog(
            action: "TenantOnboardingTemplateSave",
            entityName: "TenantOnboardingTemplate",
            actorUserId: GetUserId(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()), ct);

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
        var enabled = await _flags.GetAsync("tenant-operations.write", ct);
        if (!enabled.IsEnabled)
        {
            // Final-Touches Phase 31 Stage 31.2 — audit blocked control-plane writes.
            await _audit.LogAsync(new AuditLog(
                action: "TenantOperationsWriteBlocked",
                entityName: "TenantSubscriptionPlan",
                actorUserId: GetUserId(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()), ct);

            return StatusCode(StatusCodes.Status423Locked, new { message = "Tenant operations write path is disabled by feature flag for rollback safety." });
        }

        await _service.SaveSubscriptionPlanAsync(command, ct);

        await _audit.LogAsync(new AuditLog(
            action: "TenantSubscriptionPlanSave",
            entityName: "TenantSubscriptionPlan",
            actorUserId: GetUserId(),
            ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()), ct);

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
        var enabled = await _flags.GetAsync("tenant-operations.write", ct);
        if (!enabled.IsEnabled)
        {
            // Final-Touches Phase 31 Stage 31.2 — audit blocked control-plane writes.
            await _audit.LogAsync(new AuditLog(
                action: "TenantOperationsWriteBlocked",
                entityName: "TenantProfile",
                actorUserId: GetUserId(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString()), ct);

            return StatusCode(StatusCodes.Status423Locked, new { message = "Tenant operations write path is disabled by feature flag for rollback safety." });
        }

        await _service.SaveTenantProfileAsync(command, ct);

        await _audit.LogAsync(new AuditLog(
            action: "TenantProfileSave",
            entityName: "TenantProfile",
            actorUserId: GetUserId(),
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
