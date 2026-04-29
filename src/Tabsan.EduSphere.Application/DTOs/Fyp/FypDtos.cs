namespace Tabsan.EduSphere.Application.DTOs.Fyp;

// ── Project requests ───────────────────────────────────────────────────────

/// <summary>Request to submit a new FYP project proposal.</summary>
public record ProposeProjectRequest(
    Guid DepartmentId,
    string Title,
    string Description);

/// <summary>Request to update the title and description of a project.</summary>
public record UpdateProjectRequest(
    string Title,
    string Description);

/// <summary>Request by a coordinator to approve a project proposal.</summary>
public record ApproveProjectRequest(
    string? Remarks);

/// <summary>Request by a coordinator to reject a project proposal.</summary>
public record RejectProjectRequest(
    string Remarks);

/// <summary>Request to assign a supervisor and move the project to InProgress.</summary>
public record AssignSupervisorRequest(
    Guid SupervisorUserId);

// ── Panel member requests ──────────────────────────────────────────────────

/// <summary>Request to add a faculty member to a project panel.</summary>
public record AddPanelMemberRequest(
    Guid UserId,
    string Role);

// ── Meeting requests ───────────────────────────────────────────────────────

/// <summary>Request to schedule a new meeting for an FYP project.</summary>
public record ScheduleMeetingRequest(
    Guid FypProjectId,
    DateTime ScheduledAt,
    string Venue,
    string? Agenda);

/// <summary>Request to reschedule an existing meeting.</summary>
public record RescheduleMeetingRequest(
    DateTime ScheduledAt,
    string Venue,
    string? Agenda);

/// <summary>Request to mark a meeting as completed and record its minutes.</summary>
public record CompleteMeetingRequest(
    string? Minutes);

// ── Response DTOs ──────────────────────────────────────────────────────────

/// <summary>Summary of an FYP project for listing views.</summary>
public record FypProjectSummaryResponse(
    Guid ProjectId,
    Guid StudentProfileId,
    Guid DepartmentId,
    string Title,
    string Status,
    Guid? SupervisorUserId);

/// <summary>Full FYP project detail including panel members and meeting history.</summary>
public record FypProjectDetailResponse(
    Guid ProjectId,
    Guid StudentProfileId,
    Guid DepartmentId,
    string Title,
    string Description,
    string Status,
    Guid? SupervisorUserId,
    string? CoordinatorRemarks,
    IReadOnlyList<PanelMemberResponse> PanelMembers,
    IReadOnlyList<MeetingResponse> Meetings);

/// <summary>A single panel member assignment.</summary>
public record PanelMemberResponse(
    Guid MemberId,
    Guid UserId,
    string Role);

/// <summary>An FYP meeting as returned in project detail or schedule views.</summary>
public record MeetingResponse(
    Guid MeetingId,
    Guid FypProjectId,
    DateTime ScheduledAt,
    string Venue,
    string? Agenda,
    string Status,
    Guid OrganiserUserId,
    string? Minutes);
