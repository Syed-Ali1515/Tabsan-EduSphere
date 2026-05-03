using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Fyp;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages FYP project proposals, panel assignments, and meeting scheduling.
/// </summary>
[ApiController]
[Route("api/v1/fyp")]
[Authorize]
public sealed class FypController : ControllerBase
{
    private readonly IFypService _fypService;

    /// <summary>Initialises the controller with the FYP service.</summary>
    public FypController(IFypService fypService) => _fypService = fypService;

    // ── Projects ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Submits a new FYP project proposal. Student only.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> Propose([FromBody] ProposeProjectRequest request, CancellationToken ct)
    {
        var studentProfileId = GetStudentProfileId();
        if (studentProfileId == Guid.Empty) return Forbid();

        var id = await _fypService.ProposeAsync(request, studentProfileId, ct);
        return CreatedAtAction(nameof(GetDetail), new { id }, new { projectId = id });
    }

    /// <summary>
    /// Updates the title and description of a project (student or admin).
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken ct)
    {
        var ok = await _fypService.UpdateAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Approves a project proposal. Admin/Coordinator only.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveProjectRequest request, CancellationToken ct)
    {
        var ok = await _fypService.ApproveAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Rejects a project proposal with remarks. Admin/Coordinator only.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectProjectRequest request, CancellationToken ct)
    {
        var ok = await _fypService.RejectAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Assigns a supervisor to a project. Admin/Coordinator only.
    /// </summary>
    [HttpPost("{id:guid}/assign-supervisor")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> AssignSupervisor(Guid id, [FromBody] AssignSupervisorRequest request, CancellationToken ct)
    {
        var ok = await _fypService.AssignSupervisorAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Marks a project as completed. Admin/Coordinator only.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        var ok = await _fypService.CompleteAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all FYP projects for the current student.
    /// </summary>
    [HttpGet("my-projects")]
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> GetMyProjects(CancellationToken ct)
    {
        var studentProfileId = GetStudentProfileId();
        if (studentProfileId == Guid.Empty) return Forbid();
        return Ok(await _fypService.GetByStudentAsync(studentProfileId, ct));
    }

    /// <summary>
    /// Returns all projects in a department, optionally filtered by status.
    /// Faculty and Admin only.
    /// </summary>
    [HttpGet("by-department/{departmentId:guid}")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetByDepartment(Guid departmentId, [FromQuery] string? status, CancellationToken ct)
        => Ok(await _fypService.GetByDepartmentAsync(departmentId, status, ct));

    /// <summary>
    /// Returns projects supervised by the current faculty user.
    /// </summary>
    [HttpGet("my-supervised")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetMySupervised(CancellationToken ct)
        => Ok(await _fypService.GetBySupervisorAsync(GetCurrentUserId(), ct));

    /// <summary>
    /// Returns full project detail including panel members and meeting history.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await _fypService.GetDetailAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // ── Panel members ─────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a faculty user to a project panel. Admin/Coordinator only.
    /// </summary>
    [HttpPost("{id:guid}/panel")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> AddPanelMember(Guid id, [FromBody] AddPanelMemberRequest request, CancellationToken ct)
    {
        var ok = await _fypService.AddPanelMemberAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Removes a user from the project panel. Admin/Coordinator only.
    /// </summary>
    [HttpDelete("{id:guid}/panel/{userId:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> RemovePanelMember(Guid id, Guid userId, CancellationToken ct)
    {
        var ok = await _fypService.RemovePanelMemberAsync(id, userId, ct);
        return ok ? NoContent() : NotFound();
    }

    // ── Meetings ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Schedules a meeting for an FYP project. Faculty and Admin.
    /// </summary>
    [HttpPost("meeting")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> ScheduleMeeting([FromBody] ScheduleMeetingRequest request, CancellationToken ct)
    {
        var id = await _fypService.ScheduleMeetingAsync(request, GetCurrentUserId(), ct);
        return Ok(new { meetingId = id });
    }

    /// <summary>
    /// Reschedules an existing meeting. Faculty and Admin.
    /// </summary>
    [HttpPut("meeting/{meetingId:guid}")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> RescheduleMeeting(Guid meetingId, [FromBody] RescheduleMeetingRequest request, CancellationToken ct)
    {
        var ok = await _fypService.RescheduleMeetingAsync(meetingId, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Marks a meeting as completed and records minutes. Faculty and Admin.
    /// </summary>
    [HttpPost("meeting/{meetingId:guid}/complete")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> CompleteMeeting(Guid meetingId, [FromBody] CompleteMeetingRequest request, CancellationToken ct)
    {
        var ok = await _fypService.CompleteMeetingAsync(meetingId, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Cancels a scheduled meeting. Faculty and Admin.
    /// </summary>
    [HttpPost("meeting/{meetingId:guid}/cancel")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> CancelMeeting(Guid meetingId, CancellationToken ct)
    {
        var ok = await _fypService.CancelMeetingAsync(meetingId, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Returns all meetings for a project.
    /// </summary>
    [HttpGet("{id:guid}/meetings")]
    public async Task<IActionResult> GetMeetings(Guid id, CancellationToken ct)
        => Ok(await _fypService.GetMeetingsByProjectAsync(id, ct));

    /// <summary>
    /// Returns upcoming meetings for the current faculty supervisor.
    /// </summary>
    [HttpGet("meeting/upcoming")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetUpcomingMeetings(CancellationToken ct)
        => Ok(await _fypService.GetUpcomingMeetingsAsync(GetCurrentUserId(), ct));

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Extracts the authenticated user ID from the JWT NameIdentifier claim.</summary>
    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }

    /// <summary>Extracts the student profile ID from the "studentProfileId" JWT claim.</summary>
    private Guid GetStudentProfileId()
    {
        var value = User.FindFirstValue("studentProfileId");
        return Guid.TryParse(value, out var id) ? id : Guid.Empty;
    }
}
