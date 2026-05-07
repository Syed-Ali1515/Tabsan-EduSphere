using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Helpdesk;

/// <summary>
/// A reply/message in a support ticket thread.
/// Phase 14 — Helpdesk / Support Ticketing System.
/// </summary>
public class SupportTicketMessage : BaseEntity
{
    /// <summary>Parent ticket ID.</summary>
    public Guid TicketId { get; private set; }

    /// <summary>User ID of the author of this message.</summary>
    public Guid AuthorId { get; private set; }

    /// <summary>Message text.</summary>
    public string Body { get; private set; } = default!;

    /// <summary>Whether this message is an internal staff note (hidden from the submitter).</summary>
    public bool IsInternalNote { get; private set; }

#pragma warning disable CS8618
    private SupportTicketMessage() { }
#pragma warning restore CS8618

    public SupportTicketMessage(Guid ticketId, Guid authorId, string body, bool isInternalNote = false)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Message body is required.", nameof(body));

        TicketId       = ticketId;
        AuthorId       = authorId;
        Body           = body.Trim();
        IsInternalNote = isInternalNote;
    }
}
