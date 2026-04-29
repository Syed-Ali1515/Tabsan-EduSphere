using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages student graduation and active/inactive status lifecycle.
/// All endpoints require Admin or SuperAdmin role.
/// Routes: /api/v1/student-lifecycle
/// </summary>
[ApiController]
[Route("api/v1/student-lifecycle")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class StudentLifecycleController : ControllerBase
{
    private readonly IStudentLifecycleService _service;

    public StudentLifecycleController(IStudentLifecycleService service)
    {
        _service = service;
    }

    // ── GET /api/v1/student-lifecycle/graduation-candidates/{departmentId} ────

    /// <summary>Returns all active students in a department eligible for graduation.</summary>
    [HttpGet("graduation-candidates/{departmentId:guid}")]
    public async Task<IActionResult> GetGraduationCandidates(Guid departmentId, CancellationToken ct)
    {
        var candidates = await _service.GetGraduationCandidatesByDepartmentAsync(departmentId, ct);
        return Ok(candidates);
    }

    // ── POST /api/v1/student-lifecycle/graduate ───────────────────────────────

    /// <summary>Marks a single student as Graduated. Student dashboard becomes read-only.</summary>
    [HttpPost("graduate")]
    public async Task<IActionResult> GraduateStudent([FromBody] GraduateStudentRequest request, CancellationToken ct)
    {
        try
        {
            await _service.GraduateStudentAsync(request.StudentProfileId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
    }

    // ── POST /api/v1/student-lifecycle/graduate/batch ─────────────────────────

    /// <summary>Marks multiple students as Graduated in one request.</summary>
    [HttpPost("graduate/batch")]
    public async Task<IActionResult> GraduateStudentsBatch(
        [FromBody] IList<Guid> studentProfileIds,
        CancellationToken ct)
    {
        await _service.GraduateStudentsBatchAsync(studentProfileIds, ct);
        return NoContent();
    }

    // ── POST /api/v1/student-lifecycle/{id}/deactivate ────────────────────────

    /// <summary>Marks a student as Inactive (dropout/leave). Blocks login; preserves all academic data.</summary>
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeactivateStudentAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
    }

    // ── POST /api/v1/student-lifecycle/{id}/reactivate ────────────────────────

    /// <summary>Re-activates a previously deactivated student account.</summary>
    [HttpPost("{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.ReactivateStudentAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
    }

    // ── GET /api/v1/student-lifecycle/semester-students/{departmentId}/{semester} ──

    /// <summary>Returns all Active students in a department currently in the given semester number.</summary>
    [HttpGet("semester-students/{departmentId:guid}/{semesterNumber:int}")]
    public async Task<IActionResult> GetStudentsBySemester(
        Guid departmentId,
        int semesterNumber,
        CancellationToken ct)
    {
        var students = await _service.GetStudentsBySemesterAsync(departmentId, semesterNumber, ct);
        return Ok(students);
    }

    // ── POST /api/v1/student-lifecycle/{id}/promote ───────────────────────────

    /// <summary>Advances a single Active student to the next semester (increments CurrentSemesterNumber).</summary>
    [HttpPost("{id:guid}/promote")]
    public async Task<IActionResult> PromoteStudent(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.PromoteStudentAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return NotFound(new { message = e.Message });
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(new { message = e.Message });
        }
    }

    // ── POST /api/v1/student-lifecycle/promote/batch ──────────────────────────

    /// <summary>
    /// Advances multiple Active students to the next semester in one request.
    /// Returns a result with promoted count and per-student errors.
    /// </summary>
    [HttpPost("promote/batch")]
    public async Task<IActionResult> PromoteStudentsBatch(
        [FromBody] PromoteStudentsBatchRequest request,
        CancellationToken ct)
    {
        var result = await _service.PromoteStudentsBatchAsync(request.StudentProfileIds, ct);
        return Ok(result);
    }
}
