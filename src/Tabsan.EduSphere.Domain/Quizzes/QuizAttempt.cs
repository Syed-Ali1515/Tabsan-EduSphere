using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Quizzes;

/// <summary>
/// Lifecycle states of a student's quiz attempt.
/// </summary>
public enum AttemptStatus
{
    /// <summary>Attempt has started but not yet been submitted.</summary>
    InProgress = 1,

    /// <summary>Student explicitly submitted the attempt before time expired.</summary>
    Submitted = 2,

    /// <summary>Attempt was auto-submitted when the time limit expired.</summary>
    TimedOut = 3,

    /// <summary>Attempt was abandoned (browser closed, no submission) — scored as zero.</summary>
    Abandoned = 4
}

/// <summary>
/// Records a single attempt by a student on a <see cref="Quiz"/>.
/// Business rules:
///   - Total attempts per student must not exceed <see cref="Quiz.MaxAttempts"/> (0 = unlimited).
///   - A new attempt cannot be started while a previous InProgress attempt exists.
///   - Once submitted or timed-out, answers are immutable.
/// </summary>
public class QuizAttempt : BaseEntity
{
    /// <summary>FK to the quiz being attempted.</summary>
    public Guid QuizId { get; private set; }

    /// <summary>Navigation property to the parent quiz.</summary>
    public Quiz Quiz { get; private set; } = default!;

    /// <summary>FK to the student profile taking the quiz.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>UTC date-time when the student opened the quiz.</summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>UTC date-time when the attempt was finalised (submitted or timed out). Null while in progress.</summary>
    public DateTime? FinishedAt { get; private set; }

    /// <summary>Current lifecycle state of this attempt.</summary>
    public AttemptStatus Status { get; private set; }

    /// <summary>
    /// Aggregate score for auto-graded questions.
    /// Null until the attempt is scored. ShortAnswer questions are scored separately.
    /// </summary>
    public decimal? TotalScore { get; private set; }

    /// <summary>Navigation collection of per-question answers for this attempt.</summary>
    public IReadOnlyCollection<QuizAnswer> Answers { get; private set; } = new List<QuizAnswer>();

    // ── EF constructor ─────────────────────────────────────────────────────────
    private QuizAttempt() { }

    /// <summary>
    /// Opens a new in-progress attempt for a student.
    /// </summary>
    /// <param name="quizId">FK to the quiz.</param>
    /// <param name="studentProfileId">FK to the student starting the attempt.</param>
    public QuizAttempt(Guid quizId, Guid studentProfileId)
    {
        QuizId           = quizId;
        StudentProfileId = studentProfileId;
        StartedAt        = DateTime.UtcNow;
        Status           = AttemptStatus.InProgress;
    }

    /// <summary>
    /// Marks the attempt as submitted by the student and records the finish time.
    /// Has no effect if the attempt is already finalised.
    /// </summary>
    public void Submit()
    {
        if (Status != AttemptStatus.InProgress) return;
        Status     = AttemptStatus.Submitted;
        FinishedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Marks the attempt as timed-out (auto-submitted by the server when time expires).
    /// Has no effect if the attempt is already finalised.
    /// </summary>
    public void TimeOut()
    {
        if (Status != AttemptStatus.InProgress) return;
        Status     = AttemptStatus.TimedOut;
        FinishedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Marks the attempt as abandoned (e.g., connection lost with no submission).
    /// Has no effect if the attempt is already finalised.
    /// </summary>
    public void Abandon()
    {
        if (Status != AttemptStatus.InProgress) return;
        Status     = AttemptStatus.Abandoned;
        FinishedAt = DateTime.UtcNow;
        Touch();
    }

    /// <summary>
    /// Records the auto-graded total score after the attempt is finalised.
    /// </summary>
    /// <param name="score">Sum of marks from auto-graded correct answers.</param>
    public void RecordScore(decimal score)
    {
        TotalScore = score;
        Touch();
    }
}

/// <summary>
/// A student's response to a single <see cref="QuizQuestion"/> within a <see cref="QuizAttempt"/>.
/// For MCQ/TrueFalse: <see cref="SelectedOptionId"/> is set.
/// For ShortAnswer: <see cref="TextResponse"/> is set.
/// MarksAwarded is null until the attempt is scored.
/// </summary>
public class QuizAnswer : BaseEntity
{
    /// <summary>FK to the parent attempt.</summary>
    public Guid QuizAttemptId { get; private set; }

    /// <summary>Navigation property to the parent attempt.</summary>
    public QuizAttempt Attempt { get; private set; } = default!;

    /// <summary>FK to the question being answered.</summary>
    public Guid QuizQuestionId { get; private set; }

    /// <summary>Navigation property to the question.</summary>
    public QuizQuestion Question { get; private set; } = default!;

    /// <summary>
    /// FK to the option selected for MCQ/TrueFalse questions.
    /// Null for ShortAnswer questions.
    /// </summary>
    public Guid? SelectedOptionId { get; private set; }

    /// <summary>
    /// Free-text response for ShortAnswer questions.
    /// Null for MCQ/TrueFalse questions.
    /// </summary>
    public string? TextResponse { get; private set; }

    /// <summary>
    /// Marks awarded for this answer.
    /// Populated automatically for MCQ/TrueFalse after submit; set manually by faculty for ShortAnswer.
    /// </summary>
    public decimal? MarksAwarded { get; private set; }

    // ── EF constructor ─────────────────────────────────────────────────────────
    private QuizAnswer() { }

    /// <summary>
    /// Records a student's MCQ or TrueFalse answer by option selection.
    /// </summary>
    /// <param name="quizAttemptId">FK to the parent attempt.</param>
    /// <param name="quizQuestionId">FK to the question being answered.</param>
    /// <param name="selectedOptionId">FK to the chosen option.</param>
    public QuizAnswer(Guid quizAttemptId, Guid quizQuestionId, Guid selectedOptionId)
    {
        QuizAttemptId    = quizAttemptId;
        QuizQuestionId   = quizQuestionId;
        SelectedOptionId = selectedOptionId;
    }

    /// <summary>
    /// Records a student's ShortAnswer response.
    /// </summary>
    /// <param name="quizAttemptId">FK to the parent attempt.</param>
    /// <param name="quizQuestionId">FK to the question being answered.</param>
    /// <param name="textResponse">Free-text answer provided by the student.</param>
    public QuizAnswer(Guid quizAttemptId, Guid quizQuestionId, string textResponse)
    {
        QuizAttemptId  = quizAttemptId;
        QuizQuestionId = quizQuestionId;
        TextResponse   = textResponse;
    }

    /// <summary>
    /// Awards marks for this answer.
    /// Used by the auto-grader for MCQ/TrueFalse and by faculty for ShortAnswer.
    /// </summary>
    /// <param name="marks">Marks to award (must be &gt;= 0 and &lt;= question total marks).</param>
    public void AwardMarks(decimal marks)
    {
        MarksAwarded = marks;
        Touch();
    }
}
