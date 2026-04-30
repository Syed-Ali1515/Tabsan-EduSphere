using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Tabsan.EduSphere.Infrastructure.Licensing;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Allows Super Admin users to upload a new license file and query the current license status.
/// Admin users may read license status (read-only). Upload requires SuperAdmin.
/// Endpoints are under /api/v1/license.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class LicenseController : ControllerBase
{
    private readonly LicenseValidationService _licenseService;

    // Temporary directory where uploaded license files are saved before validation.
    private static readonly string UploadDirectory =
        Path.Combine(Path.GetTempPath(), "tabsan_license_uploads");

    public LicenseController(LicenseValidationService licenseService)
    {
        _licenseService = licenseService;
        Directory.CreateDirectory(UploadDirectory);
    }

    // ── POST /api/v1/license/upload ───────────────────────────────────────────

    /// <summary>
    /// Accepts a license file upload, saves it to a temp location,
    /// validates the RSA signature, and activates the license on success.
    /// Returns 400 Bad Request when the file fails validation.
    /// File size is capped at 64 KB to prevent oversized upload abuse.
    /// </summary>
    [HttpPost("upload")]
    [Authorize(Roles = "SuperAdmin")]
    [RequestSizeLimit(65_536)] // 64 KB max license file
    public async Task<IActionResult> Upload(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No file provided." });

        if (!file.FileName.EndsWith(".tablic", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "License file must be a .tablic file." });

        // Save to a temp path with a random name to prevent path traversal.
        var tempFile = Path.Combine(UploadDirectory, $"{Guid.NewGuid()}.tablic");

        try
        {
            await using var stream = System.IO.File.Create(tempFile);
            await file.CopyToAsync(stream, ct);
        }
        catch
        {
            return StatusCode(500, new { message = "Failed to save the uploaded file." });
        }

        var success = await _licenseService.ActivateFromFileAsync(tempFile, ct);

        // Clean up the temp file regardless of outcome.
        System.IO.File.Delete(tempFile);

        if (!success)
            return BadRequest(new { message = "License validation failed. The file may be invalid or tampered." });

        return Ok(new { message = "License activated successfully." });
    }

    // ── GET /api/v1/license/status ────────────────────────────────────────────

    /// <summary>
    /// Runs an on-demand license check and returns the current status.
    /// Useful for health checks and the Super Admin dashboard.
    /// </summary>
    [HttpGet("status")]
    public async Task<IActionResult> Status(CancellationToken ct)
    {
        var status = await _licenseService.ValidateCurrentAsync(ct);
        return Ok(new { status = status.ToString() });
    }

    // ── GET /api/v1/license/details ───────────────────────────────────────────

    /// <summary>
    /// Returns full license detail: status, type, activation date, expiry, remaining days.
    /// Available to both Super Admin (read + upload) and Admin (read-only).
    /// </summary>
    [HttpGet("details")]
    public async Task<IActionResult> Details(CancellationToken ct)
    {
        var state = await _licenseService.GetCurrentStateAsync(ct);
        if (state is null)
            return Ok(new { status = "None", licenseType = (string?)null, activatedAt = (DateTime?)null, expiresAt = (DateTime?)null, remainingDays = (int?)null, updatedAt = (DateTime?)null });

        int? remainingDays = state.ExpiresAt.HasValue
            ? (int)Math.Max(0, Math.Ceiling((state.ExpiresAt.Value - DateTime.UtcNow).TotalDays))
            : null;

        return Ok(new
        {
            status       = state.Status.ToString(),
            licenseType  = state.LicenseType.ToString(),
            activatedAt  = state.ActivatedAt,
            expiresAt    = state.ExpiresAt,
            updatedAt    = state.UpdatedAt,
            remainingDays
        });
    }
}
