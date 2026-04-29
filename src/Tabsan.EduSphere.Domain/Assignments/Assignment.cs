using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Assignments;

/// <summary>
/// An assignment created by a faculty member for a specific <see cref="CourseOffering"/>.
/// Once published it is visible to enrolled students. It can be soft-deleted (retracted)
/// before any submissions are made but never after.
/// </summary>
public class Assignment : AuditableEntity
{
    /// <summary>FK to the course offering this assignment belongs to.</summary>
    public Guid CourseOfferingId { get; private set; }

    /// <summary>Short title shown in student dashboards (e.g. "Lab 1 – Linked Lists").</summary>
    public string Title { get; private set; } = default!;

    /// <summary>Full instructions / problem statement. May contain Markdown.</summary>
    public string? Description { get; private set; }

    /// <summary>UTC deadline after which submissions are rejected.</summary>
    public DateTime DueDate { get; private set; }

    /// <summary>Maximum marks available. Used to validate awarded marks.</summary>
    public decimal MaxMarks { get; private set; }

    /// <summary>
    /// True once the assignment has been published and students can see and submit.
    /// Unpublished assignments are visible to faculty only.
    /// </summary>
    public bool IsPublished { get; private set; }

    /// <summary>UTC timestamp of when the assignment was published.</summary>
    public DateTime? PublishedAt { get; private set; }

    private Assignment() { }

    /// <summary>
    /// Creates a new, unpublished assignment.
    /// The assignment must be explicitly published before students can see it.
    /// </summary>
    public Assignment(Guid courseOfferingId, string title, string? description, DateTime dueDate, decimal maxMarks)
    {
        CourseOfferingId = courseOfferingId;
        Title = title.Trim();
        Description = description?.Trim();
        DueDate = dueDate;
        MaxMarks = maxMarks > 0
            ? maxMarks
            : throw new ArgumentOutOfRangeException(nameof(maxMarks), "MaxMarks must be greater than 0.");
    }

    /// <summary>
    /// Publishes the assignment so enrolled students can view and submit.
    /// Throws if already published.
    /// </summary>
    public void Publish()
    {
        if (IsPublished)
            throw new InvalidOperationException("Assignment is already published.");

        IsPublished = true;
        PublishedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Retracts a published assignment (sets IsPublished to false).
    /// Only allowed before any submissions exist — enforced at the service layer.
    /// </summary>
    public void Retract()
    {
        if (!IsPublished)
            throw new InvalidOperationException("Assignment is not published.");

        IsPublished = false;
        PublishedAt = null;
        Touch();
    }

    /// <summary>
    /// Updates the assignment's editable fields.
    /// Only allowed before publishing.
    /// </summary>
    public void Update(string title, string? description, DateTime dueDate, decimal maxMarks)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot edit a published assignment.");

        Title = title.Trim();
        Description = description?.Trim();
        DueDate = dueDate;
        MaxMarks = maxMarks > 0
            ? maxMarks
            : throw new ArgumentOutOfRangeException(nameof(maxMarks), "MaxMarks must be greater than 0.");
        Touch();
    }
}
