using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Allows any authenticated user to read and update their own UI theme preference.
/// The ThemeKey is stored per-user in the database; the front-end applies the CSS theme.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ThemeController : ControllerBase
{
    private readonly IThemeService _service;

    public ThemeController(IThemeService service) => _service = service;

    // ── GET /api/v1/theme ─────────────────────────────────────────────────────

    /// <summary>Returns the current authenticated user's theme key. Null = system default.</summary>
    [HttpGet]
    public async Task<IActionResult> GetTheme(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        try
        {
            var dto = await _service.GetThemeAsync(userId, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── PUT /api/v1/theme ─────────────────────────────────────────────────────

    /// <summary>Sets (or clears) the current user's theme preference.</summary>
    [HttpPut]
    public async Task<IActionResult> SetTheme([FromBody] SetThemeCommand cmd, CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Forbid();

        try
        {
            await _service.SetThemeAsync(userId, cmd, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid GetUserId()
    {
        var raw = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
