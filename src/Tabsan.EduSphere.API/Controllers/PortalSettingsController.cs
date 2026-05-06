using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

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
    private readonly IWebHostEnvironment _env;

    public PortalSettingsController(IPortalBrandingService service, IWebHostEnvironment env)
    {
        _service = service;
        _env     = env;
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
    /// Uploads a logo image (PNG/JPG/SVG/GIF ≤ 2 MB) and stores it in wwwroot/portal-uploads/.
    /// Returns the relative URL of the saved file.  SuperAdmin only.
    /// </summary>
    [HttpPost("logo")]
    [Authorize(Roles = "SuperAdmin")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file supplied." });

        const long maxBytes = 2 * 1024 * 1024; // 2 MB
        if (file.Length > maxBytes)
            return BadRequest(new { message = "Logo must be ≤ 2 MB." });

        var allowed = new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp" };
        var ext     = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(new { message = "Allowed types: PNG, JPG, GIF, SVG, WEBP." });

        var webRoot = _env.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(_env.ContentRootPath, "wwwroot");
        }

        var uploadsDir = Path.Combine(webRoot, "portal-uploads");
        Directory.CreateDirectory(uploadsDir);

        // Always write to a fixed name so only one logo exists at a time.
        var fileName   = $"logo{ext}";
        var filePath   = Path.Combine(uploadsDir, fileName);
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await file.CopyToAsync(stream, ct);

        var relativeUrl = $"/portal-uploads/{fileName}";
        return Ok(new { url = relativeUrl });
    }
}
