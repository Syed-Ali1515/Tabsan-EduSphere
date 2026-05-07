using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tabsan.EduSphere.Application.DTOs.Search;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Cross-entity global search endpoint for Phase 13.
/// Accessible to all authenticated roles; results are scoped to the caller's role.
/// </summary>
[ApiController]
[Route("api/v1/search")]
[Authorize]
public sealed class SearchController : ControllerBase
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService) => _searchService = searchService;

    /// <summary>
    /// Executes a global search across students, courses, offerings, faculty, and departments.
    /// Results are role-scoped automatically based on the JWT caller.
    /// </summary>
    /// <param name="q">Search term (minimum 2 characters).</param>
    /// <param name="limit">Maximum results to return (1–50, default 20).</param>
    /// <param name="ct">Cancellation token.</param>
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int    limit = 20,
        CancellationToken  ct    = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return BadRequest(new { error = "Search term must be at least 2 characters." });

        if (!Guid.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out var callerId))
            return Unauthorized();

        var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

        var clampedLimit = Math.Clamp(limit, 1, 50);

        var request  = new SearchRequest(callerId, role, q.Trim(), clampedLimit);
        var response = await _searchService.SearchAsync(request, ct);

        return Ok(response);
    }
}
