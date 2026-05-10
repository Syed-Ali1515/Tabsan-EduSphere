using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Phase 27.3 — exposes active integration provider contracts.

[ApiController]
[Route("api/v1/communication-integrations")]
[Authorize]
public sealed class CommunicationIntegrationsController : ControllerBase
{
    private readonly ICommunicationIntegrationService _service;
    private readonly IOutboundIntegrationGateway _gateway;

    public CommunicationIntegrationsController(
        ICommunicationIntegrationService service,
        IOutboundIntegrationGateway gateway)
    {
        _service = service;
        _gateway = gateway;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var profile = await _service.GetProfileAsync(ct);
        return Ok(profile);
    }

    [HttpGet("gateway/policies")]
    public IActionResult GetGatewayPolicies()
    {
        var channels = new[] { "payment", "email", "sms", "push", "lms-external-api" };
        var policies = channels.Select(_gateway.GetPolicySnapshot).ToList();
        return Ok(policies);
    }

    [HttpGet("gateway/dead-letters")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetGatewayDeadLetters([FromQuery] int take = 50, CancellationToken ct = default)
    {
        var entries = await _gateway.GetRecentDeadLettersAsync(take, ct);
        var total = await _gateway.GetDeadLetterCountAsync(ct);
        return Ok(new { total, items = entries });
    }
}
