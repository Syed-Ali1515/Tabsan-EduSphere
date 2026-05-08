// Final-Touches Phase 22 Stage 22.1 — LibraryController: library system integration API
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.External;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages the external library system integration.
/// SuperAdmin configures the catalogue URL and API token.
/// Authenticated users can retrieve their loan status via the proxy endpoint.
/// </summary>
[ApiController]
[Route("api/v1/library")]
[Authorize]
public class LibraryController : ControllerBase
{
    private readonly ILibraryService _service;

    public LibraryController(ILibraryService service) => _service = service;

    /// <summary>Returns current library connection configuration. SuperAdmin only.</summary>
    [HttpGet("config")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetConfig(CancellationToken ct)
    {
        var config = await _service.GetConfigAsync(ct);
        return Ok(config);
    }

    /// <summary>Saves the library connection settings. SuperAdmin only.</summary>
    [HttpPut("config")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> SaveConfig([FromBody] SaveLibraryConfigCommand cmd, CancellationToken ct)
    {
        await _service.SaveConfigAsync(cmd, ct);
        return NoContent();
    }

    /// <summary>
    /// Proxies a loan-status request to the configured external library API.
    /// Uses the calling user's username as the student identifier.
    /// </summary>
    [HttpGet("loans")]
    public async Task<IActionResult> GetLoans(CancellationToken ct)
    {
        var identifier = User.FindFirstValue(ClaimTypes.Name)
                         ?? User.FindFirstValue(ClaimTypes.Email)
                         ?? "";
        var result = await _service.GetLoansAsync(identifier, ct);
        return Ok(result);
    }

    /// <summary>
    /// Proxies a loan-status request for a specific student identifier.
    /// Admin and SuperAdmin can look up any student.
    /// </summary>
    [HttpGet("loans/{studentIdentifier}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetLoansForStudent(string studentIdentifier, CancellationToken ct)
    {
        var result = await _service.GetLoansAsync(studentIdentifier, ct);
        return Ok(result);
    }
}
