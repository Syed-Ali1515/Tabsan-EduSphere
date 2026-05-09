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

    public CommunicationIntegrationsController(ICommunicationIntegrationService service)
    {
        _service = service;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var profile = await _service.GetProfileAsync(ct);
        return Ok(profile);
    }
}
