using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tabsan.EduSphere.Application.DTOs.StudyPlanner;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

// Final-Touches Phase 21 Stage 21.1/21.2 — Study Planner REST API

/// <summary>
/// REST API for student semester study plans (Phase 21).
/// Students create and manage their own plans.
/// Faculty/Admin can advise (endorse or reject) plans.
/// Recommendations are available to Student and Faculty roles.
/// </summary>
[ApiController]
[Route("api/v1/study-plan")]
[Authorize]
public class StudyPlanController : ControllerBase
{
    private readonly IStudyPlanService _svc;
    public StudyPlanController(IStudyPlanService svc) => _svc = svc;

    // ── Stage 21.1: Plan CRUD ─────────────────────────────────────────────────

    /// <summary>Returns all study plans for the given student profile.</summary>
    [HttpGet("plans/{studentProfileId:guid}")]
    [Authorize(Roles = "Student,Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> GetPlans(Guid studentProfileId, CancellationToken ct = default)
    {
        var plans = await _svc.GetPlansAsync(studentProfileId, ct);
        return Ok(plans);
    }

    /// <summary>Returns all plans for all students in a department (faculty advisor view).</summary>
    [HttpGet("plans/department/{departmentId:guid}")]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> GetPlansByDepartment(Guid departmentId, CancellationToken ct = default)
    {
        var plans = await _svc.GetPlansByDepartmentAsync(departmentId, ct);
        return Ok(plans);
    }

    /// <summary>Returns a single study plan by its ID.</summary>
    [HttpGet("plan/{planId:guid}")]
    public async Task<IActionResult> GetPlan(Guid planId, CancellationToken ct = default)
    {
        var plan = await _svc.GetPlanAsync(planId, ct);
        return plan is null ? NotFound() : Ok(plan);
    }

    /// <summary>Creates a new semester study plan for a student.</summary>
    [HttpPost("plan")]
    [Authorize(Roles = "Student,Admin,SuperAdmin")]
    public async Task<IActionResult> CreatePlan([FromBody] CreateStudyPlanRequest request, CancellationToken ct = default)
    {
        var plan = await _svc.CreatePlanAsync(request, ct);
        return CreatedAtAction(nameof(GetPlan), new { planId = plan.Id }, plan);
    }

    /// <summary>Adds a course to a study plan (validates prerequisites and credit load).</summary>
    [HttpPost("plan/{planId:guid}/course")]
    [Authorize(Roles = "Student,Admin,SuperAdmin")]
    public async Task<IActionResult> AddCourse(Guid planId, [FromBody] Guid courseId, CancellationToken ct = default)
    {
        var request = new AddPlanCourseRequest(planId, courseId);
        var plan = await _svc.AddCourseAsync(request, ct);
        return Ok(plan);
    }

    /// <summary>Removes a course from a study plan.</summary>
    [HttpDelete("plan/{planId:guid}/course/{courseId:guid}")]
    [Authorize(Roles = "Student,Admin,SuperAdmin")]
    public async Task<IActionResult> RemoveCourse(Guid planId, Guid courseId, CancellationToken ct = default)
    {
        await _svc.RemoveCourseAsync(planId, courseId, ct);
        return NoContent();
    }

    /// <summary>Soft-deletes a study plan.</summary>
    [HttpDelete("plan/{planId:guid}")]
    [Authorize(Roles = "Student,Admin,SuperAdmin")]
    public async Task<IActionResult> DeletePlan(Guid planId, CancellationToken ct = default)
    {
        await _svc.DeletePlanAsync(planId, ct);
        return NoContent();
    }

    // ── Stage 21.1: Advisor workflow ──────────────────────────────────────────

    /// <summary>Faculty advisor endorses or rejects a student's study plan.</summary>
    [HttpPost("plan/{planId:guid}/advise")]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> AdvisePlan(Guid planId, [FromBody] AdvisePlanRequest request, CancellationToken ct = default)
    {
        var userId = Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : Guid.Empty;
        // Ensure request uses the planId from the route
        var normalised = request with { PlanId = planId };
        await _svc.AdvisePlanAsync(normalised, userId, ct);
        return NoContent();
    }

    // ── Stage 21.2: Recommendations ──────────────────────────────────────────

    /// <summary>Returns auto-generated course recommendations for the student's next planned semester.</summary>
    [HttpGet("recommendations/{studentProfileId:guid}")]
    [Authorize(Roles = "Student,Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> GetRecommendations(
        Guid   studentProfileId,
        [FromQuery] string plannedSemesterName = "Next Semester",
        CancellationToken ct = default)
    {
        var result = await _svc.GetRecommendationsAsync(studentProfileId, plannedSemesterName, ct);
        return Ok(result);
    }
}
