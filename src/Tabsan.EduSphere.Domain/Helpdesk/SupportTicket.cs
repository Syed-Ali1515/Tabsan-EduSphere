using Tabsan.EduSphere.Domain.Common;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Domain.Helpdesk;

/// <summary>
/// A support ticket raised by a Student or Faculty member.
/// Phase 14 — Helpdesk / Support Ticketing System.
/// </summary>
public class SupportTicket : AuditableEntity
{
    /// <summary>User ID of the person who submitted the ticket.</summary>
    public Guid SubmitterId { get; private set; }

    /// <summary>Department the submitter belongs to at submission time (used for Admin scope filtering).</summary>
    public Guid? DepartmentId { get; private set; }

    /// <summary>Ticket category (Academic / Technical / Administrative).</summary>
    public TicketCategory Category { get; private set; }

    /// <summary>One-line subject of the ticket.</summary>
    public string Subject { get; private set; } = default!;

    /// <summary>Full description / body of the initial request.</summary>
    public string Body { get; private set; } = default!;

    /// <summary>Current lifecycle status.</summary>
    public TicketStatus Status { get; private set; } = TicketStatus.Open;

    /// <summary>User ID of the staff member (Admin / Faculty) the ticket is assigned to. Null = unassigned.</summary>
    public Guid? AssignedToId { get; private set; }

    /// <summary>UTC timestamp when the ticket was resolved or closed. Null while open.</summary>
    public DateTime? ResolvedAt { get; private set; }

    /// <summary>Number of days within which the submitter may re-open a Resolved ticket (0 = re-open not allowed).</summary>
    public int ReopenWindowDays { get; private set; } = 3;

    /// <summary>Thread messages on this ticket.</summary>
    public IReadOnlyCollection<SupportTicketMessage> Messages { get; private set; } = new List<SupportTicketMessage>();

#pragma warning disable CS8618
    private SupportTicket() { }
#pragma warning restore CS8618

    public SupportTicket(Guid submitterId, Guid? departmentId, TicketCategory category,
        string subject, string body, int reopenWindowDays = 3)
    {
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required.", nameof(body));

        SubmitterId      = submitterId;
        DepartmentId     = departmentId;
        Category         = category;
        Subject          = subject.Trim();
        Body             = body.Trim();
        ReopenWindowDays = Math.Max(0, reopenWindowDays);
    }

    /// <summary>Assigns the ticket to a staff member and transitions to InProgress if still Open.</summary>
    public void Assign(Guid assignedToId)
    {
        AssignedToId = assignedToId;
        if (Status == TicketStatus.Open)
            Status = TicketStatus.InProgress;
        Touch();
    }

    /// <summary>Marks the ticket as Resolved.</summary>
    public void Resolve()
    {
        if (Status == TicketStatus.Closed)
            throw new InvalidOperationException("Cannot resolve a closed ticket.");

        Status     = TicketStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>Permanently closes the ticket (no further re-opens).</summary>
    public void Close()
    {
        Status = TicketStatus.Closed;
        Touch();
    }

    /// <summary>
    /// Allows the original submitter to re-open a Resolved ticket within the allowed window.
    /// </summary>
    public void Reopen()
    {
        if (Status != TicketStatus.Resolved)
            throw new InvalidOperationException("Only Resolved tickets can be re-opened.");

        if (ReopenWindowDays > 0 && ResolvedAt.HasValue)
        {
            var deadline = ResolvedAt.Value.AddDays(ReopenWindowDays);
            if (DateTime.UtcNow > deadline)
                throw new InvalidOperationException(
                    $"Re-open window of {ReopenWindowDays} day(s) has expired.");
        }

        Status     = TicketStatus.Open;
        ResolvedAt = null;
        Touch();
    }

    /// <summary>Updates the ticket status directly (SuperAdmin override).</summary>
    public void SetStatus(TicketStatus status)
    {
        Status = status;
        if (status == TicketStatus.Resolved && ResolvedAt is null)
            ResolvedAt = DateTime.UtcNow;
        Touch();
    }
}
