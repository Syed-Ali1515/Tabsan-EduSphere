using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Assignments;

/// <summary>
/// Current lifecycle state of a student's assignment submission.
/// </summary>
public enum SubmissionStatus
{
    /// <summary>Submitted by the student and awaiting faculty review.</summary>
    Submitted = 1,

    /// <summary>Marks and optional feedback have been entered by the faculty member.</summary>
    Graded = 2,

    /// <summary>Submission was rejected (e.g. submitted after deadline by override).</summary>
    Rejected = 3
}

/// <summary>
/// A student's submission for a specific <see cref="Assignment"/>.
/// There is at most one submission per student per assignment (enforced by unique index).
/// Submission rows are NEVER deleted — they form permanent academic evidence.
/// </summary>
public class AssignmentSubmission : BaseEntity
{
    /// <summary>FK to the assignment being submitted.</summary>
    public Guid AssignmentId { get; private set; }

    /// <summary>Navigation to the parent assignment.</summary>
    public Assignment Assignment { get; private set; } = default!;

    /// <summary>FK to the student profile who made the submission.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>
    /// Optional URL or path to the uploaded file stored in blob/object storage.
    /// Null when the submission is text-only.
    /// </summary>
    public string? FileUrl { get; private set; }

    /// <summary>
    /// Optional free-text body of the submission.
    /// Null when the submission is file-only.
    /// </summary>
    public string? TextContent { get; private set; }

    /// <summary>UTC timestamp of when the student pressed Submit.</summary>
    public DateTime SubmittedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Marks awarded by the faculty member.
    /// Null until the submission is graded.
    /// Must be between 0 and the parent Assignment's MaxMarks.
    /// </summary>
    public decimal? MarksAwarded { get; private set; }

    /// <summary>Optional written feedback from the faculty member.</summary>
    public string? Feedback { get; private set; }

    /// <summary>UTC timestamp of when marks were awarded.</summary>
    public DateTime? GradedAt { get; private set; }

    /// <summary>FK to the faculty user who graded this submission.</summary>
    public Guid? GradedByUserId { get; private set; }

    /// <summary>Current lifecycle state of this submission.</summary>
    public SubmissionStatus Status { get; private set; } = SubmissionStatus.Submitted;

    private AssignmentSubmission() { }

    /// <summary>
    /// Creates a new submission. At least one of fileUrl or textContent must be provided.
    /// </summary>
    public AssignmentSubmission(Guid assignmentId, Guid studentProfileId, string? fileUrl, string? textContent)
    {
        if (string.IsNullOrWhiteSpace(fileUrl) && string.IsNullOrWhiteSpace(textContent))
            throw new ArgumentException("A submission must have either a file URL or text content.");

        AssignmentId = assignmentId;
        StudentProfileId = studentProfileId;
        FileUrl = fileUrl?.Trim();
        TextContent = textContent?.Trim();
    }

    /// <summary>
    /// Records marks and optional feedback for this submission.
    /// Marks must be between 0 and the assignment's max marks — validated at the service layer.
    /// </summary>
    public void Grade(decimal marksAwarded, string? feedback, Guid gradedByUserId)
    {
        if (Status == SubmissionStatus.Rejected)
            throw new InvalidOperationException("Cannot grade a rejected submission.");

        MarksAwarded = marksAwarded;
        Feedback = feedback?.Trim();
        GradedAt = DateTime.UtcNow;
        GradedByUserId = gradedByUserId;
        Status = SubmissionStatus.Graded;
    }

    /// <summary>
    /// Rejects the submission (e.g. plagiarism or late override).
    /// Sets status to Rejected; previously awarded marks are cleared.
    /// </summary>
    public void Reject()
    {
        Status = SubmissionStatus.Rejected;
        MarksAwarded = null;
        GradedAt = null;
    }
}
