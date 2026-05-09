using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
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
    private readonly MediaStorageOptions _mediaStorageOptions;

    public PortalSettingsController(
        IPortalBrandingService service,
        IMediaStorageService mediaStorage,
        IOptions<MediaStorageOptions> mediaStorageOptions)
    {
        _service = service;
        _mediaStorage = mediaStorage;
        _mediaStorageOptions = mediaStorageOptions.Value;
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
    /// Uploads a logo image (PNG/JPG/SVG/GIF ≤ 2 MB) and returns a logo URL.
    /// The caller stores this URL in portal_settings for DB-backed branding. SuperAdmin only.
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
    public async Task<IActionResult> GetLogoFile(
        string storageKey,
        [FromQuery] long? exp,
        [FromQuery] string? sig,
        CancellationToken ct)
    {
        // Final-Touches Phase 28 Stage 28.3 — enforce signed reads while preserving unsigned-link compatibility.
        if (string.IsNullOrWhiteSpace(storageKey))
            return NotFound();

        // Only permit logo category keys through this public endpoint.
        if (!storageKey.Contains("portal-branding/logo", StringComparison.OrdinalIgnoreCase))
            return NotFound();

        if (IsSignedReadRequired())
        {
            if (!exp.HasValue || string.IsNullOrWhiteSpace(sig))
                return Redirect(BuildLocalSignedLogoUrl(storageKey, TimeSpan.FromMinutes(10)));

            if (exp.Value < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                return NotFound();

            if (!IsValidLocalSignature(storageKey, exp.Value, sig))
                return NotFound();
        }

        var temporaryUrl = await _mediaStorage.GenerateTemporaryReadUrlAsync(storageKey, TimeSpan.FromMinutes(10), ct);
        if (Uri.TryCreate(temporaryUrl, UriKind.Absolute, out _))
            return Redirect(temporaryUrl!);

        var metadata = await _mediaStorage.GetMetadataAsync(storageKey, ct);
        var bytes = await _mediaStorage.ReadAsBytesAsync(storageKey, ct);
        if (bytes is null) return NotFound();

        return File(bytes, metadata?.ContentType ?? ResolveImageContentType(storageKey));
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

    private bool IsSignedReadRequired() => !string.IsNullOrWhiteSpace(_mediaStorageOptions.SignedUrlSecret);

    private string BuildLocalSignedLogoUrl(string storageKey, TimeSpan ttl)
    {
        var expiresAt = DateTimeOffset.UtcNow.Add(ttl <= TimeSpan.Zero ? TimeSpan.FromMinutes(10) : ttl).ToUnixTimeSeconds();
        var signature = CreateSignature(storageKey, expiresAt);

        var escapedKey = Uri.EscapeDataString(storageKey).Replace("%2F", "/", StringComparison.OrdinalIgnoreCase);
        return $"/api/v1/portal-settings/logo-files/{escapedKey}?exp={expiresAt}&sig={Uri.EscapeDataString(signature)}";
    }

    private bool IsValidLocalSignature(string storageKey, long expiresAt, string providedSignature)
    {
        var expected = CreateSignature(storageKey, expiresAt);
        var normalized = providedSignature.Trim();

        if (!TryDecodeHex(expected, out var expectedBytes)) return false;
        if (!TryDecodeHex(normalized, out var providedBytes)) return false;

        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }

    private string CreateSignature(string storageKey, long expiresAt)
    {
        var secret = _mediaStorageOptions.SignedUrlSecret;
        if (string.IsNullOrWhiteSpace(secret))
            return string.Empty;

        var payload = $"{storageKey}|{expiresAt}";
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(payload);

        using var hmac = new HMACSHA256(keyBytes);
        return Convert.ToHexString(hmac.ComputeHash(payloadBytes)).ToLowerInvariant();
    }

    private static bool TryDecodeHex(string value, out byte[] bytes)
    {
        try
        {
            bytes = Convert.FromHexString(value);
            return true;
        }
        catch
        {
            bytes = Array.Empty<byte>();
            return false;
        }
    }
}
