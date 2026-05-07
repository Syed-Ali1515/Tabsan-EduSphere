using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.API.Services;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Reads and writes institution-wide portal branding settings.
/// GET is open to any authenticated user; POST/logo are restricted to SuperAdmin.
/// </summary>
[ApiController]
[Route("api/v1/portal-settings")]
[Authorize]
public class PortalSettingsController : ControllerBase
{
    private readonly IPortalBrandingService _service;

    public PortalSettingsController(IPortalBrandingService service)
    {
        _service = service;
    }

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

    /// <summary>
    /// Uploads a logo image (PNG/JPG/SVG/GIF ≤ 2 MB) and returns a data URI string.
    /// The caller stores this value in portal_settings for DB-backed branding. SuperAdmin only.
    /// </summary>
    [HttpPost("logo")]
    [Authorize(Roles = "SuperAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken ct)
    {
        var error = await FileUploadValidator.ValidateImageAsync(file);
        if (error is not null)
            return BadRequest(new { message = error });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        await using var read = file.OpenReadStream();
        await using var ms = new MemoryStream();
        await read.CopyToAsync(ms, ct);

        var mime = ext switch
        {
            ".png"  => "image/png",
            ".jpg"  => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif"  => "image/gif",
            ".svg"  => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };

        var b64     = Convert.ToBase64String(ms.ToArray());
        var dataUri = $"data:{mime};base64,{b64}";
        return Ok(new { url = dataUri });
    }
}
