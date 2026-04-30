using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
public class SidebarMenuController : ControllerBase
{
    private readonly ISidebarMenuService _service;

    public SidebarMenuController(ISidebarMenuService service) => _service = service;

    // ── GET /api/v1/sidebar-menu/my-visible ───────────────────────────────────

    /// <summary>
    /// Returns sidebar menu items visible to the current authenticated user.
    /// SuperAdmin always sees all menu items regardless of role/status rules.
    /// </summary>
    [HttpGet("my-visible")]
    [Authorize]
    public async Task<IActionResult> GetVisibleForCurrentUser(CancellationToken ct)
    {
        if (User.IsInRole("SuperAdmin"))
        {
            var allMenus = await _service.GetTopLevelMenusAsync(ct);
            return Ok(allMenus.OrderBy(m => m.DisplayOrder));
        }

        var effectiveRoles = User.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role")
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var filteredRoles = effectiveRoles
            .Where(r => r is "Admin" or "Faculty" or "Student")
            .ToList();

        if (filteredRoles.Count == 0)
            return Ok(Array.Empty<SidebarMenuItemDto>());

        var topLevel = await _service.GetTopLevelMenusAsync(ct);
        var visible = FilterVisible(topLevel, filteredRoles);
        return Ok(visible);
    }

    // ── GET /api/v1/sidebar-menu ──────────────────────────────────────────────

    /// <summary>Returns all top-level menu items with sub-menus and role access lists.</summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetTopLevel(CancellationToken ct)
    {
        var items = await _service.GetTopLevelMenusAsync(ct);
        return Ok(items);
    }

    // ── GET /api/v1/sidebar-menu/{id} ─────────────────────────────────────────

    /// <summary>Returns a single menu item by ID with full role access detail.</summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
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
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetSubMenus(Guid id, CancellationToken ct)
    {
        var items = await _service.GetSubMenusAsync(id, ct);
        return Ok(items);
    }

    // ── PUT /api/v1/sidebar-menu/{id}/roles ───────────────────────────────────

    /// <summary>
    /// Replaces the role access assignments for a menu item.
    /// </summary>
    [HttpPut("{id:guid}/roles")]
    [Authorize(Roles = "SuperAdmin")]
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
    [Authorize(Roles = "SuperAdmin")]
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

    private static IList<SidebarMenuItemDto> FilterVisible(IEnumerable<SidebarMenuItemDto> topLevelMenus, IList<string> roles)
    {
        var result = new List<SidebarMenuItemDto>();

        foreach (var menu in topLevelMenus.OrderBy(m => m.DisplayOrder))
        {
            var visibleSubMenus = menu.SubMenus
                .Where(s => IsVisibleForRoles(s, roles))
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            // Keep parent if parent itself is visible OR at least one child is visible.
            if (IsVisibleForRoles(menu, roles) || visibleSubMenus.Count > 0)
                result.Add(menu with { SubMenus = visibleSubMenus });
        }

        return result;
    }

    private static bool IsVisibleForRoles(SidebarMenuItemDto menu, IList<string> roles)
    {
        if (!menu.IsActive) return false;

        return menu.RoleAccesses.Any(a =>
            a.IsAllowed && roles.Contains(a.RoleName, StringComparer.OrdinalIgnoreCase));
    }
}
