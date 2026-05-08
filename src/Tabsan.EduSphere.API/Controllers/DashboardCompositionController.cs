using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.API.Middleware;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Returns the ordered dashboard widget list for the authenticated user.
/// </summary>
[ApiController]
[Route("api/v1/dashboard")]
[Authorize]
public sealed class DashboardCompositionController : ControllerBase
{
    private readonly IDashboardCompositionService _composer;

    public DashboardCompositionController(IDashboardCompositionService composer)
        => _composer = composer;

    /// <summary>Returns widget descriptors for the current user's role and institution context.</summary>
    [HttpGet("composition")]
    public IActionResult GetComposition()
    {
        var role   = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                     ?? User.FindFirst("role")?.Value ?? "";
        var policy = HttpContext.GetInstitutionPolicy();
        var widgets = _composer.GetWidgets(role, policy);

        return Ok(widgets.Select(w => new
        {
            w.Key,
            w.Title,
            w.Icon,
            w.Order
        }));
    }
}
