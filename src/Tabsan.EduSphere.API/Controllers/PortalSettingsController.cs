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
    private readonly IMediaStorageService _mediaStorage;

    public PortalSettingsController(IPortalBrandingService service, IMediaStorageService mediaStorage)
    {
        _service = service;
        _mediaStorage = mediaStorage;
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
        // Final-Touches Phase 28 Stage 28.3 — persist portal logo through storage provider.
        var error = await FileUploadValidator.ValidateImageAsync(file);
        if (error is not null)
            return BadRequest(new { message = error });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        await using var read = file.OpenReadStream();
        var stored = await _mediaStorage.SaveAsync(read, "portal-branding/logo", ext, ct);

        return Ok(new { url = $"/api/v1/portal-settings/logo-files/{stored.StorageKey}" });
    }

    /// <summary>
    /// Returns a stored portal logo by storage key.
    /// Kept anonymous so login/landing pages can render branding without bearer headers.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("logo-files/{**storageKey}")]
    public async Task<IActionResult> GetLogoFile(string storageKey, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(storageKey))
            return NotFound();

        // Only permit logo category keys through this public endpoint.
        if (!storageKey.Contains("portal-branding/logo", StringComparison.OrdinalIgnoreCase))
            return NotFound();

        var bytes = await _mediaStorage.ReadAsBytesAsync(storageKey, ct);
        if (bytes is null) return NotFound();

        return File(bytes, ResolveImageContentType(storageKey));
    }

    private static string ResolveImageContentType(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".png" => "image/png",
            ".jpg" => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
