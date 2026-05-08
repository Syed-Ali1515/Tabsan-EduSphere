using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Lms;

// Final-Touches Phase 20 Stage 20.3 — discussion thread for a course offering

/// <summary>
/// A discussion thread opened by a student or faculty member within a <see cref="CourseOffering"/>.
/// Faculty can pin and close threads; any participant can reply.
/// </summary>
public class DiscussionThread : AuditableEntity
{
    /// <summary>FK to the course offering this thread belongs to.</summary>
    public Guid OfferingId { get; private set; }

    /// <summary>Thread subject line shown in the listing.</summary>
    public string Title { get; private set; } = default!;

    /// <summary>UserId of the user who opened the thread.</summary>
    public Guid AuthorId { get; private set; }

    /// <summary>Pinned threads appear at the top of the forum listing.</summary>
    public bool IsPinned { get; private set; }

    /// <summary>Closed threads accept no new replies.</summary>
    public bool IsClosed { get; private set; }

    // Navigation
    public ICollection<DiscussionReply> Replies { get; private set; } = new List<DiscussionReply>();

    private DiscussionThread() { }

    /// <summary>Creates a new open, unpinned discussion thread.</summary>
    public DiscussionThread(Guid offeringId, Guid authorId, string title)
    {
        OfferingId = offeringId;
        AuthorId   = authorId;
        Title      = title.Trim();
    }

    /// <summary>Faculty toggles the pinned state of a thread.</summary>
    public void SetPinned(bool pinned)
    {
        IsPinned = pinned;
        Touch();
    }

    /// <summary>Faculty closes a thread to prevent further replies.</summary>
    public void Close()
    {
        IsClosed = true;
        Touch();
    }

    /// <summary>Faculty re-opens a previously closed thread.</summary>
    public void Reopen()
    {
        IsClosed = false;
        Touch();
    }
}
