using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages sidebar navigation menu items and their per-role visibility settings.
/// Super Admin always bypasses role restrictions — these settings apply only to
/// Admin, Faculty, and Student roles.
/// </summary>
[ApiController]
[Route("api/v1/sidebar-menu")]
[Authorize(Roles = "SuperAdmin")]
public class SidebarMenuController : ControllerBase
{
    private readonly ISidebarMenuService _service;

    public SidebarMenuController(ISidebarMenuService service) => _service = service;

    // ── GET /api/v1/sidebar-menu ──────────────────────────────────────────────

    /// <summary>Returns all top-level menu items with sub-menus and role access lists.</summary>
    [HttpGet]
    public async Task<IActionResult> GetTopLevel(CancellationToken ct)
    {
        var items = await _service.GetTopLevelMenusAsync(ct);
        return Ok(items);
    }

    // ── GET /api/v1/sidebar-menu/{id} ─────────────────────────────────────────

    /// <summary>Returns a single menu item by ID with full role access detail.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var dto = await _service.GetByIdAsync(id, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET /api/v1/sidebar-menu/{id}/sub-menus ───────────────────────────────

    /// <summary>Returns the sub-menu items under a given top-level menu item.</summary>
    [HttpGet("{id:guid}/sub-menus")]
    public async Task<IActionResult> GetSubMenus(Guid id, CancellationToken ct)
    {
        var items = await _service.GetSubMenusAsync(id, ct);
        return Ok(items);
    }

    // ── PUT /api/v1/sidebar-menu/{id}/roles ───────────────────────────────────

    /// <summary>
    /// Replaces the role access assignments for a menu item.
    /// Roles not included in the payload are left unchanged.
    /// </summary>
    [HttpPut("{id:guid}/roles")]
    public async Task<IActionResult> SetRoles(Guid id, [FromBody] SetSidebarMenuRolesCommand cmd, CancellationToken ct)
    {
        try
        {
            await _service.SetRolesAsync(id, cmd, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── PUT /api/v1/sidebar-menu/{id}/status ──────────────────────────────────

    /// <summary>
    /// Activates or deactivates a menu item.
    /// System menus cannot be deactivated — a 409 Conflict is returned if attempted.
    /// </summary>
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid id, [FromBody] SetSidebarMenuStatusCommand cmd, CancellationToken ct)
    {
        try
        {
            await _service.SetStatusAsync(id, cmd, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
