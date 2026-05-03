using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Quizzes;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages quiz authoring, publishing, attempt lifecycle, and grading.
/// </summary>
[ApiController]
[Route("api/v1/quiz")]
[Authorize]
public sealed class QuizController : ControllerBase
{
    private readonly IQuizService _quizService;

    /// <summary>Initialises the controller with the quiz service.</summary>
    public QuizController(IQuizService quizService) => _quizService = quizService;

    // ── Authoring ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new unpublished quiz for a course offering.
    /// Accessible by Faculty and Admin.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> Create([FromBody] CreateQuizRequest request, CancellationToken ct)
    {
        var id = await _quizService.CreateAsync(request, GetCurrentUserId(), ct);
        return CreatedAtAction(nameof(GetDetail), new { id }, new { quizId = id });
    }

    /// <summary>
    /// Updates the metadata of an unpublished quiz.
    /// Accessible by Faculty and Admin.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateQuizRequest request, CancellationToken ct)
    {
        var ok = await _quizService.UpdateAsync(id, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Publishes a quiz, making it visible and accessible to students.
    /// Accessible by Faculty and Admin.
    /// </summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        var ok = await _quizService.PublishAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Unpublishes a quiz, hiding it from students without deleting it.
    /// Accessible by Faculty and Admin.
    /// </summary>
    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
    {
        var ok = await _quizService.UnpublishAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Soft-deletes a quiz.
    /// Accessible by Admin only.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var ok = await _quizService.DeactivateAsync(id, ct);
        return ok ? NoContent() : NotFound();
    }

    // ── Question management ───────────────────────────────────────────────────

    /// <summary>
    /// Adds a question (with options) to a quiz.
    /// Accessible by Faculty and Admin.
    /// </summary>
    [HttpPost("question")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> AddQuestion([FromBody] AddQuestionRequest request, CancellationToken ct)
    {
        var id = await _quizService.AddQuestionAsync(request, ct);
        return Ok(new { questionId = id });
    }

    /// <summary>
    /// Updates an existing question's text, marks, and ordering.
    /// Accessible by Faculty and Admin.
    /// </summary>
    [HttpPut("question/{questionId:guid}")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> UpdateQuestion(Guid questionId, [FromBody] UpdateQuestionRequest request, CancellationToken ct)
    {
        var ok = await _quizService.UpdateQuestionAsync(questionId, request, ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>
    /// Removes a question and all its options.
    /// Accessible by Faculty and Admin.
    /// </summary>
    [HttpDelete("question/{questionId:guid}")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> RemoveQuestion(Guid questionId, CancellationToken ct)
    {
        var ok = await _quizService.RemoveQuestionAsync(questionId, ct);
        return ok ? NoContent() : NotFound();
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all active quizzes for a course offering.
    /// Accessible by all authenticated users.
    /// </summary>
    [HttpGet("by-offering/{courseOfferingId:guid}")]
    public async Task<IActionResult> GetByOffering(Guid courseOfferingId, CancellationToken ct)
        => Ok(await _quizService.GetByOfferingAsync(courseOfferingId, ct));

    /// <summary>
    /// Returns full quiz detail with questions and options.
    /// Faculty and Admin see correct answer flags; students see options without IsCorrect.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id, CancellationToken ct)
    {
        var result = await _quizService.GetDetailAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // ── Attempts ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Opens a new quiz attempt for the current student.
    /// Returns 409 when the attempt cap has been reached or an attempt is in-progress.
    /// </summary>
    [HttpPost("{id:guid}/start")]
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> StartAttempt(Guid id, CancellationToken ct)
    {
        var studentProfileId = GetStudentProfileId();
        if (studentProfileId == Guid.Empty) return Forbid();

        var attempt = await _quizService.StartAttemptAsync(id, studentProfileId, ct);
        return attempt is null ? Conflict("Quiz unavailable or attempt cap reached.") : Ok(attempt);
    }

    /// <summary>
    /// Submits an in-progress attempt with all answers and returns the scored result.
    /// </summary>
    [HttpPost("attempt/submit")]
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> SubmitAttempt([FromBody] SubmitAttemptRequest request, CancellationToken ct)
    {
        var result = await _quizService.SubmitAttemptAsync(request, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Returns all attempts the current student has made across all quizzes (portal summary).
    /// </summary>
    [HttpGet("my-attempts")]
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> GetAllMyAttempts(CancellationToken ct)
    {
        var studentProfileId = GetStudentProfileId();
        if (studentProfileId == Guid.Empty) return Forbid();
        return Ok(await _quizService.GetAllMyAttemptsAsync(studentProfileId, ct));
    }

    /// <summary>
    /// Returns all attempts the current student has made on a quiz.
    /// </summary>
    [HttpGet("{id:guid}/my-attempts")]
    [Authorize(Policy = "Student")]
    public async Task<IActionResult> GetMyAttempts(Guid id, CancellationToken ct)
    {
        var studentProfileId = GetStudentProfileId();
        if (studentProfileId == Guid.Empty) return Forbid();
        return Ok(await _quizService.GetStudentAttemptsAsync(id, studentProfileId, ct));
    }

    /// <summary>
    /// Returns full attempt detail including answers.
    /// Faculty/Admin can view any attempt; students can only view their own.
    /// </summary>
    [HttpGet("attempt/{attemptId:guid}")]
    public async Task<IActionResult> GetAttemptDetail(Guid attemptId, CancellationToken ct)
    {
        var result = await _quizService.GetAttemptDetailAsync(attemptId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Awards marks for a ShortAnswer response (faculty manual grading).
    /// </summary>
    [HttpPost("attempt/grade-answer")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GradeAnswer([FromBody] GradeAnswerRequest request, CancellationToken ct)
    {
        var ok = await _quizService.GradeAnswerAsync(request, ct);
        return ok ? NoContent() : NotFound();
    }

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
