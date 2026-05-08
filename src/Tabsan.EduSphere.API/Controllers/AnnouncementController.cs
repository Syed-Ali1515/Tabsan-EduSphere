using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tabsan.EduSphere.Application.DTOs.Lms;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Final-Touches Phase 20 Stage 20.4 — course announcement REST API

/// <summary>
/// REST API for course announcements (Phase 20).
/// All authenticated users can read announcements.
/// Faculty/Admin/SuperAdmin can create and delete.
/// </summary>
[ApiController]
[Route("api/v1/announcement")]
[Authorize]
public class AnnouncementController : ControllerBase
{
    private readonly IAnnouncementService _announcements;
    public AnnouncementController(IAnnouncementService announcements) => _announcements = announcements;

    /// <summary>Returns all announcements for the given offering.</summary>
    [HttpGet("{offeringId:guid}")]
    public async Task<IActionResult> GetAnnouncements(Guid offeringId, CancellationToken ct = default)
    {
        var items = await _announcements.GetByOfferingAsync(offeringId, ct);
        return Ok(items);
    }

    /// <summary>Posts an announcement (notifies enrolled students). Faculty/Admin/SuperAdmin only.</summary>
    [HttpPost]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> CreateAnnouncement(
        [FromBody] CreateAnnouncementRequest request, CancellationToken ct = default)
    {
        var idStr    = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        if (!Guid.TryParse(idStr, out var authorId)) return Unauthorized();
        var actualRequest = request with { AuthorId = authorId };
        var item = await _announcements.CreateAsync(actualRequest, ct);
        return Ok(item);
    }

    /// <summary>Soft-deletes an announcement. Faculty/Admin/SuperAdmin only.</summary>
    [HttpDelete("{announcementId:guid}")]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteAnnouncement(Guid announcementId, CancellationToken ct = default)
    {
        await _announcements.DeleteAsync(announcementId, ct);
        return NoContent();
    }
}
