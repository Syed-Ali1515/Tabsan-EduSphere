using Tabsan.EduSphere.Application.DTOs.Helpdesk;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Enums;
using Tabsan.EduSphere.Domain.Helpdesk;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.Application.Helpdesk;

/// <summary>
/// Application service for Phase 14 — Helpdesk / Support Ticketing System.
/// Delegates data access to IHelpdeskRepository; dispatches in-app notifications
/// via INotificationService on status changes.
/// </summary>
public sealed class HelpdeskService : IHelpdeskService
{
    private readonly IHelpdeskRepository   _helpdesk;
    private readonly IUserRepository       _users;
    private readonly INotificationService  _notifications;

    public HelpdeskService(
        IHelpdeskRepository  helpdesk,
        IUserRepository      users,
        INotificationService notifications)
    {
        _helpdesk      = helpdesk;
        _users         = users;
        _notifications = notifications;
    }

    // ── Submission / Viewing ──────────────────────────────────────────────────

    public async Task<Guid> CreateTicketAsync(CreateTicketRequest request, CancellationToken ct = default)
    {
        var ticket = new SupportTicket(
            request.SubmitterId,
            request.DepartmentId,
            request.Category,
            request.Subject,
            request.Body);

        await _helpdesk.AddTicketAsync(ticket, ct);
        await _helpdesk.SaveChangesAsync(ct);
        return ticket.Id;
    }

    public async Task<TicketDetailDto?> GetTicketDetailAsync(
        Guid ticketId, Guid callerId, string callerRole, CancellationToken ct = default)
    {
        var ticket = await _helpdesk.GetTicketByIdAsync(ticketId, ct);
        if (ticket is null) return null;

        if (!CanViewTicket(ticket, callerId, callerRole)) return null;

        // Staff can see internal notes; submitters cannot
        bool showInternalNotes = callerRole is "SuperAdmin" or "Admin" or "Faculty";
        var messages = await _helpdesk.GetMessagesAsync(ticketId, showInternalNotes, ct);

        var submitter = await _users.GetByIdAsync(ticket.SubmitterId, ct);
        var assignee  = ticket.AssignedToId.HasValue
            ? await _users.GetByIdAsync(ticket.AssignedToId.Value, ct)
            : null;

        var messageDtos = await BuildMessageDtosAsync(messages, ct);

        bool canReopen = ticket.Status == TicketStatus.Resolved
            && ticket.ReopenWindowDays > 0
            && ticket.ResolvedAt.HasValue
            && DateTime.UtcNow <= ticket.ResolvedAt.Value.AddDays(ticket.ReopenWindowDays)
            && callerId == ticket.SubmitterId;

        return new TicketDetailDto(
            ticket.Id,
            ticket.Subject,
            ticket.Body,
            ticket.Category,
            ticket.Status,
            ticket.SubmitterId,
            submitter?.Username ?? "Unknown",
            ticket.AssignedToId,
            assignee?.Username,
            ticket.DepartmentId,
            ticket.CreatedAt,
            ticket.ResolvedAt,
            ticket.ReopenWindowDays,
            canReopen,
            messageDtos);
    }

    public async Task<IReadOnlyList<TicketSummaryDto>> GetTicketsAsync(
        Guid callerId, string callerRole,
        IReadOnlyList<Guid>? departmentIds, TicketStatus? status, CancellationToken ct = default)
    {
        IReadOnlyList<SupportTicket> tickets;

        if (callerRole is "SuperAdmin")
        {
            tickets = await _helpdesk.GetTicketsByDepartmentAsync(null, status, ct);
        }
        else if (callerRole is "Admin")
        {
            tickets = await _helpdesk.GetTicketsByDepartmentAsync(departmentIds, status, ct);
        }
        else if (callerRole is "Faculty")
        {
            // Faculty sees tickets assigned to them + their own submissions
            var assigned    = await _helpdesk.GetTicketsByAssigneeAsync(callerId, ct);
            var submitted   = await _helpdesk.GetTicketsBySubmitterAsync(callerId, ct);
            tickets = assigned.Concat(submitted)
                              .DistinctBy(t => t.Id)
                              .OrderByDescending(t => t.CreatedAt)
                              .ToList();
        }
        else
        {
            // Student — own submissions only
            tickets = await _helpdesk.GetTicketsBySubmitterAsync(callerId, ct);
        }

        // Apply status filter client-side for Faculty/Student (already applied server-side for Admin/SuperAdmin)
        if (status.HasValue && callerRole is "Faculty" or "Student")
            tickets = tickets.Where(t => t.Status == status.Value).ToList();

        var summaries = new List<TicketSummaryDto>(tickets.Count);
        foreach (var t in tickets)
        {
            var submitter = await _users.GetByIdAsync(t.SubmitterId, ct);
            var assignee  = t.AssignedToId.HasValue
                ? await _users.GetByIdAsync(t.AssignedToId.Value, ct)
                : null;
            var msgCount = (await _helpdesk.GetMessagesAsync(t.Id, includeInternalNotes: false, ct)).Count;
            summaries.Add(new TicketSummaryDto(
                t.Id, t.Subject, t.Category, t.Status,
                t.SubmitterId, submitter?.Username ?? "Unknown",
                t.AssignedToId, assignee?.Username,
                t.DepartmentId, t.CreatedAt, t.ResolvedAt, msgCount));
        }
        return summaries;
    }

    // ── Messaging ────────────────────────────────────────────────────────────

    public async Task<Guid> AddMessageAsync(AddMessageRequest request, CancellationToken ct = default)
    {
        var ticket = await _helpdesk.GetTicketByIdAsync(request.TicketId, ct)
            ?? throw new InvalidOperationException("Ticket not found.");

        var message = new SupportTicketMessage(
            request.TicketId, request.AuthorId, request.Body, request.IsInternalNote);

        await _helpdesk.AddMessageAsync(message, ct);
        await _helpdesk.SaveChangesAsync(ct);

        // Notify: if author is not the submitter, notify the submitter
        if (!request.IsInternalNote && request.AuthorId != ticket.SubmitterId)
        {
            var author = await _users.GetByIdAsync(request.AuthorId, ct);
            await _notifications.SendSystemAsync(
                $"New reply on ticket: {ticket.Subject}",
                $"{author?.Username ?? "Staff"} replied to your support ticket.",
                NotificationType.System,
                new[] { ticket.SubmitterId },
                ct);
        }

        return message.Id;
    }

    // ── Case Management ───────────────────────────────────────────────────────

    public async Task AssignTicketAsync(AssignTicketRequest request, CancellationToken ct = default)
    {
        var ticket = await _helpdesk.GetTicketByIdAsync(request.TicketId, ct)
            ?? throw new InvalidOperationException("Ticket not found.");

        ticket.Assign(request.AssignedToId);
        await _helpdesk.SaveChangesAsync(ct);

        // Notify the assignee
        var assignee = await _users.GetByIdAsync(request.AssignedToId, ct);
        if (assignee is not null)
        {
            await _notifications.SendSystemAsync(
                "Support ticket assigned to you",
                $"Ticket \"{ticket.Subject}\" has been assigned to you.",
                NotificationType.System,
                new[] { request.AssignedToId },
                ct);
        }
    }

    public async Task ResolveTicketAsync(Guid ticketId, Guid callerId, string callerRole, CancellationToken ct = default)
    {
        var ticket = await _helpdesk.GetTicketByIdAsync(ticketId, ct)
            ?? throw new InvalidOperationException("Ticket not found.");

        if (!CanManageTicket(ticket, callerId, callerRole))
            throw new UnauthorizedAccessException("Not authorised to resolve this ticket.");

        ticket.Resolve();
        await _helpdesk.SaveChangesAsync(ct);

        await _notifications.SendSystemAsync(
            "Your support ticket has been resolved",
            $"Ticket \"{ticket.Subject}\" has been marked as resolved.",
            NotificationType.System,
            new[] { ticket.SubmitterId },
            ct);
    }

    public async Task CloseTicketAsync(Guid ticketId, Guid callerId, string callerRole, CancellationToken ct = default)
    {
        var ticket = await _helpdesk.GetTicketByIdAsync(ticketId, ct)
            ?? throw new InvalidOperationException("Ticket not found.");

        if (!CanManageTicket(ticket, callerId, callerRole))
            throw new UnauthorizedAccessException("Not authorised to close this ticket.");

        ticket.Close();
        await _helpdesk.SaveChangesAsync(ct);
    }

    public async Task ReopenTicketAsync(Guid ticketId, Guid submitterId, CancellationToken ct = default)
    {
        var ticket = await _helpdesk.GetTicketByIdAsync(ticketId, ct)
            ?? throw new InvalidOperationException("Ticket not found.");

        if (ticket.SubmitterId != submitterId)
            throw new UnauthorizedAccessException("Only the original submitter can re-open a ticket.");

        ticket.Reopen();
        await _helpdesk.SaveChangesAsync(ct);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private static bool CanViewTicket(SupportTicket ticket, Guid callerId, string role) =>
        role is "SuperAdmin" or "Admin"
        || ticket.SubmitterId == callerId
        || ticket.AssignedToId == callerId;

    private static bool CanManageTicket(SupportTicket ticket, Guid callerId, string role) =>
        role is "SuperAdmin" or "Admin"
        || ticket.AssignedToId == callerId;

    private async Task<IReadOnlyList<TicketMessageDto>> BuildMessageDtosAsync(
        IReadOnlyList<SupportTicketMessage> messages, CancellationToken ct)
    {
        var result = new List<TicketMessageDto>(messages.Count);
        foreach (var m in messages)
        {
            var author = await _users.GetByIdAsync(m.AuthorId, ct);
            result.Add(new TicketMessageDto(
                m.Id, m.AuthorId, author?.Username ?? "Unknown",
                m.Body, m.IsInternalNote, m.CreatedAt));
        }
        return result;
    }
}
