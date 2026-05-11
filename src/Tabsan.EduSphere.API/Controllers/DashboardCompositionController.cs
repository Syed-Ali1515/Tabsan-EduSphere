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
    private readonly IModuleRegistryService _moduleRegistry;
    private readonly ILabelService _labelService;

    public DashboardCompositionController(
        IDashboardCompositionService composer,
        IModuleRegistryService moduleRegistry,
        ILabelService labelService)
    {
        _composer = composer;
        _moduleRegistry = moduleRegistry;
        _labelService = labelService;
    }

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

    /// <summary>
    /// Returns the aggregated dashboard composition payload used by the portal
    /// module composition screen to avoid three separate round-trips.
    /// </summary>
    [HttpGet("context")]
    public async Task<IActionResult> GetContext(CancellationToken ct)
    {
        var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                   ?? User.FindFirst("role")?.Value ?? "";
        var policy = HttpContext.GetInstitutionPolicy();

        var modulesTask = _moduleRegistry.GetVisibleModulesAsync(role, policy, ct);
        var widgets = _composer.GetWidgets(role, policy);
        var vocabulary = _labelService.GetVocabulary(policy);

        var modules = await modulesTask;

        return Ok(new
        {
            Modules = modules.Select(m => new
            {
                m.Key,
                m.Name,
                m.IsActive,
                m.IsAccessible
            }).ToList(),
            Vocabulary = new
            {
                vocabulary.PeriodLabel,
                vocabulary.ProgressionLabel,
                vocabulary.GradingLabel,
                vocabulary.CourseLabel,
                vocabulary.StudentGroupLabel
            },
            Widgets = widgets.Select(w => new
            {
                w.Key,
                w.Title,
                w.Icon,
                w.Order
            }).ToList()
        });
    }
}
