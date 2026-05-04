using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Notifications;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages notification compose, dispatch, inbox queries, and read-state.
/// Admins/Faculty: send and deactivate notifications.
/// All authenticated users: view inbox and mark notifications read.
/// </summary>
// Final-Touches Phase 6 Stage 6.1 — fixed route to include v1 prefix (was "api/[controller]")
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _service;
    public NotificationController(INotificationService service) => _service = service;

    // ── Send ──────────────────────────────────────────────────────────────────

    /// <summary>Sends a notification to a specific list of users (Admin/Faculty).</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var id = await _service.SendAsync(request, userId, ct);
        return Ok(new { notificationId = id });
    }

    /// <summary>Deactivates a notification, hiding it from all inboxes (Admin only).</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var ok = await _service.DeactivateAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    // ── Inbox ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns paged inbox notifications for the current user.
    /// Use ?unreadOnly=true to filter to unread notifications only.
    /// </summary>
    [HttpGet("inbox")]
    public async Task<IActionResult> GetInbox(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var items = await _service.GetInboxAsync(userId, unreadOnly, page, pageSize, ct);
        return Ok(items);
    }

    /// <summary>Returns the unread notification count for the badge indicator.</summary>
    [HttpGet("badge")]
    public async Task<IActionResult> GetBadge(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var badge = await _service.GetBadgeAsync(userId, ct);
        return Ok(badge);
    }

    // ── Read state ────────────────────────────────────────────────────────────

    /// <summary>Marks a single notification as read for the current user.</summary>
    [HttpPost("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var ok = await _service.MarkReadAsync(id, userId, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Marks all unread notifications as read for the current user.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty) return Unauthorized();

        await _service.MarkAllReadAsync(userId, ct);
        return NoContent();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Extracts the authenticated user's ID from JWT claims.</summary>
    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
