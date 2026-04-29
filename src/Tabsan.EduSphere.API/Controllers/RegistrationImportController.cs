using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Allows admins to import registration numbers into the whitelist via CSV
/// or add individual entries manually.
/// Routes: /api/v1/registration-import
/// </summary>
[ApiController]
[Route("api/v1/registration-import")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class RegistrationImportController : ControllerBase
{
    private readonly ICsvRegistrationImportService _importService;

    public RegistrationImportController(ICsvRegistrationImportService importService)
    {
        _importService = importService;
    }

    // ── POST /api/v1/registration-import/csv ─────────────────────────────────

    /// <summary>
    /// Imports registration numbers in bulk from a CSV file.
    /// Expected columns (header required): RegistrationNumber,DepartmentId,ProgramId
    /// Returns a summary with imported count, duplicates, and validation errors.
    /// </summary>
    [HttpPost("csv")]
    public async Task<IActionResult> ImportCsv(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "No CSV file provided." });

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".csv")
            return BadRequest(new { message = "Only .csv files are accepted." });

        await using var stream = file.OpenReadStream();
        var result = await _importService.ImportFromCsvAsync(stream, ct);
        return Ok(result);
    }

    // ── POST /api/v1/registration-import/single ───────────────────────────────

    /// <summary>
    /// Adds a single registration number to the whitelist manually.
    /// Returns 409 Conflict if the registration number already exists.
    /// </summary>
    [HttpPost("single")]
    public async Task<IActionResult> AddSingle(
        [FromBody] SingleRegistrationRequest request,
        CancellationToken ct)
    {
        var added = await _importService.AddSingleAsync(
            request.RegistrationNumber,
            request.DepartmentId,
            request.ProgramId,
            ct);

        if (!added)
            return Conflict(new { message = $"Registration number '{request.RegistrationNumber}' already exists in the whitelist." });

        return Ok(new { message = "Registration number added successfully." });
    }
}

/// <summary>Request body for adding a single registration number to the whitelist.</summary>
public record SingleRegistrationRequest(
    string RegistrationNumber,
    Guid DepartmentId,
    Guid ProgramId
);
