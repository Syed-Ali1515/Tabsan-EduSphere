using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Reads and writes institution-wide portal branding settings.
/// GET is open to any authenticated user; POST is restricted to SuperAdmin.
/// </summary>
[ApiController]
[Route("api/v1/portal-settings")]
[Authorize]
public class PortalSettingsController : ControllerBase
{
    private readonly IPortalBrandingService _service;

    public PortalSettingsController(IPortalBrandingService service) => _service = service;

    /// <summary>Returns the current portal branding values.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var dto = await _service.GetAsync(ct);
        return Ok(dto);
    }

    /// <summary>Saves (upserts) all portal branding fields. SuperAdmin only.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Save([FromBody] SavePortalBrandingCommand cmd, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(cmd.UniversityName))
            return BadRequest(new { message = "University name is required." });

        await _service.SaveAsync(cmd, ct);
        return NoContent();
    }
}
