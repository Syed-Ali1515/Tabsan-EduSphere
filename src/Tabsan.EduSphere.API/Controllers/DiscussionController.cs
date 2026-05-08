using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tabsan.EduSphere.Application.DTOs.Lms;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Final-Touches Phase 20 Stage 20.3 — discussion forum REST API

/// <summary>
/// REST API for course discussion threads and replies (Phase 20).
/// All authenticated users can read and create threads/replies.
/// Faculty/Admin/SuperAdmin have additional moderation capabilities.
/// </summary>
[ApiController]
[Route("api/v1/discussion")]
[Authorize]
public class DiscussionController : ControllerBase
{
    private readonly IDiscussionService _discussion;
    public DiscussionController(IDiscussionService discussion) => _discussion = discussion;

    // ── Threads ────────────────────────────────────────────────────────────────

    /// <summary>Returns all discussion threads for the given offering.</summary>
    [HttpGet("{offeringId:guid}/threads")]
    public async Task<IActionResult> GetThreads(Guid offeringId, CancellationToken ct = default)
    {
        var threads = await _discussion.GetThreadsAsync(offeringId, ct);
        return Ok(threads);
    }

    /// <summary>Returns a thread with all replies.</summary>
    [HttpGet("thread/{threadId:guid}")]
    public async Task<IActionResult> GetThread(Guid threadId, CancellationToken ct = default)
    {
        var thread = await _discussion.GetThreadAsync(threadId, ct);
        return thread is null ? NotFound() : Ok(thread);
    }

    /// <summary>Creates a new discussion thread. Any authenticated user can post.</summary>
    [HttpPost("thread")]
    public async Task<IActionResult> CreateThread([FromBody] CreateThreadRequest request, CancellationToken ct = default)
    {
        var callerId = ExtractCallerId();
        if (callerId == Guid.Empty) return Unauthorized();
        var actualRequest = request with { AuthorId = callerId };
        var thread = await _discussion.CreateThreadAsync(actualRequest, ct);
        return CreatedAtAction(nameof(GetThread), new { threadId = thread.Id }, thread);
    }

    /// <summary>Pins or unpins a thread. Faculty/Admin/SuperAdmin only.</summary>
    [HttpPost("thread/{threadId:guid}/pin")]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> SetPinned(Guid threadId, [FromQuery] bool pinned = true, CancellationToken ct = default)
    {
        await _discussion.SetPinnedAsync(threadId, pinned, ct);
        return NoContent();
    }

    /// <summary>Closes a thread to new replies. Faculty/Admin/SuperAdmin only.</summary>
    [HttpPost("thread/{threadId:guid}/close")]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> CloseThread(Guid threadId, CancellationToken ct = default)
    {
        await _discussion.CloseThreadAsync(threadId, ct);
        return NoContent();
    }

    /// <summary>Reopens a closed thread. Faculty/Admin/SuperAdmin only.</summary>
    [HttpPost("thread/{threadId:guid}/reopen")]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> ReopenThread(Guid threadId, CancellationToken ct = default)
    {
        await _discussion.ReopenThreadAsync(threadId, ct);
        return NoContent();
    }

    /// <summary>Soft-deletes a thread. Faculty/Admin/SuperAdmin only.</summary>
    [HttpDelete("thread/{threadId:guid}")]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteThread(Guid threadId, CancellationToken ct = default)
    {
        await _discussion.DeleteThreadAsync(threadId, ct);
        return NoContent();
    }

    // ── Replies ────────────────────────────────────────────────────────────────

    /// <summary>Posts a reply. Any authenticated user can reply.</summary>
    [HttpPost("reply")]
    public async Task<IActionResult> AddReply([FromBody] AddReplyRequest request, CancellationToken ct = default)
    {
        var callerId = ExtractCallerId();
        if (callerId == Guid.Empty) return Unauthorized();
        var actualRequest = request with { AuthorId = callerId };
        var reply = await _discussion.AddReplyAsync(actualRequest, ct);
        return Ok(reply);
    }

    /// <summary>Soft-deletes a reply. The author or Faculty/Admin/SuperAdmin can delete.</summary>
    [HttpDelete("reply/{replyId:guid}")]
    public async Task<IActionResult> DeleteReply(Guid replyId, CancellationToken ct = default)
    {
        var requesterId = ExtractCallerId();
        var role        = User.FindFirstValue(ClaimTypes.Role) ?? "";
        bool isFaculty  = role is "Faculty" or "Admin" or "SuperAdmin";
        await _discussion.DeleteReplyAsync(replyId, requesterId, isFaculty, ct);
        return NoContent();
    }

    private Guid ExtractCallerId()
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
        return Guid.TryParse(idStr, out var id) ? id : Guid.Empty;
    }
}
