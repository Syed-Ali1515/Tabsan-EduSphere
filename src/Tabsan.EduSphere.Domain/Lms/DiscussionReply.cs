using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Lms;

// Final-Touches Phase 20 Stage 20.3 — reply within a discussion thread

/// <summary>
/// A reply posted by a user within a <see cref="DiscussionThread"/>.
/// </summary>
public class DiscussionReply : AuditableEntity
{
    /// <summary>FK to the parent discussion thread.</summary>
    public Guid ThreadId { get; private set; }

    /// <summary>UserId of the reply author.</summary>
    public Guid AuthorId { get; private set; }

    /// <summary>Reply body text (plain text or Markdown).</summary>
    public string Body { get; private set; } = default!;

    // Navigation
    public DiscussionThread Thread { get; private set; } = default!;

    private DiscussionReply() { }

    /// <summary>Creates a new discussion reply.</summary>
    public DiscussionReply(Guid threadId, Guid authorId, string body)
    {
        ThreadId = threadId;
        AuthorId = authorId;
        Body     = body.Trim();
    }

    /// <summary>Updates the reply body (author can edit their own post).</summary>
    public void UpdateBody(string body)
    {
        Body = body.Trim();
        Touch();
    }
}
