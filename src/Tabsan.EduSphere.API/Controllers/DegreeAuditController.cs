// Final-Touches Phase 17 Stage 17.1/17.2/17.3 — Degree Audit API controller

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Degree Audit endpoints: credit-completion audits, graduation eligibility checks,
/// degree rule CRUD, and course-type tagging.
/// </summary>
[ApiController]
[Route("api/v1/degree-audit")]
[Authorize]
public class DegreeAuditController : ControllerBase
{
    private readonly IDegreeAuditService      _degreeAudit;
    private readonly IStudentProfileRepository _studentRepo;

    public DegreeAuditController(
        IDegreeAuditService       degreeAudit,
        IStudentProfileRepository studentRepo)
    {
        _degreeAudit = degreeAudit;
        _studentRepo = studentRepo;
    }

    // ── GET /api/v1/degree-audit/me ───────────────────────────────────────────

    // Final-Touches Phase 17 Stage 17.1 — student retrieves own audit
    /// <summary>Returns the degree audit for the currently authenticated student.</summary>
    [HttpGet("me")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyAudit(CancellationToken ct)
    {
        var userId  = GetUserId();
        var profile = await _studentRepo.GetByUserIdAsync(userId, ct);
        if (profile is null) return NotFound("Student profile not found.");
        var audit = await _degreeAudit.GetAuditAsync(profile.Id, ct);
        return Ok(audit);
    }

    // ── GET /api/v1/degree-audit/{studentProfileId} ───────────────────────────

    // Final-Touches Phase 17 Stage 17.1 — admin/faculty retrieves audit for any student
    /// <summary>Returns the degree audit for the specified student profile (Admin/Faculty/SuperAdmin).</summary>
    [HttpGet("{studentProfileId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin,Faculty")]
    public async Task<IActionResult> GetStudentAudit(Guid studentProfileId, CancellationToken ct)
    {
        try
        {
            var audit = await _degreeAudit.GetAuditAsync(studentProfileId, ct);
            return Ok(audit);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ── GET /api/v1/degree-audit/eligible ────────────────────────────────────

    // Final-Touches Phase 17 Stage 17.2 — eligibility list for admin
    /// <summary>Returns a graduation eligibility summary for all students (Admin/SuperAdmin).</summary>
    [HttpGet("eligible")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetEligibilityList(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? programId,
        CancellationToken ct)
    {
        var list = await _degreeAudit.GetEligibilityListAsync(departmentId, programId, ct);
        return Ok(list);
    }

    // ── GET /api/v1/degree-audit/rule ────────────────────────────────────────

    // Final-Touches Phase 17 Stage 17.2 — list all rules for SuperAdmin
    /// <summary>Returns all configured degree rules (SuperAdmin).</summary>
    [HttpGet("rule")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllRules(CancellationToken ct)
    {
        var rules = await _degreeAudit.GetAllRulesAsync(ct);
        return Ok(rules);
    }

    // ── GET /api/v1/degree-audit/rule/{programId} ─────────────────────────────

    // Final-Touches Phase 17 Stage 17.2 — any authenticated user can view their program's rule
    /// <summary>Returns the degree rule for the specified academic program.</summary>
    [HttpGet("rule/{programId:guid}")]
    public async Task<IActionResult> GetRuleByProgram(Guid programId, CancellationToken ct)
    {
        var rule = await _degreeAudit.GetRuleByProgramAsync(programId, ct);
        return rule is null ? NotFound("No degree rule is configured for this program.") : Ok(rule);
    }

    // ── POST /api/v1/degree-audit/rule ───────────────────────────────────────

    // Final-Touches Phase 17 Stage 17.2 — SuperAdmin creates a new degree rule
    /// <summary>Creates a new degree rule for an academic program (SuperAdmin).</summary>
    [HttpPost("rule")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateRule([FromBody] CreateDegreeRuleRequest request, CancellationToken ct)
    {
        var rule = await _degreeAudit.CreateRuleAsync(request, ct);
        return CreatedAtAction(nameof(GetRuleByProgram), new { programId = rule.AcademicProgramId }, rule);
    }

    // ── PUT /api/v1/degree-audit/rule/{ruleId} ────────────────────────────────

    // Final-Touches Phase 17 Stage 17.2 — SuperAdmin updates an existing degree rule
    /// <summary>Updates an existing degree rule (SuperAdmin).</summary>
    [HttpPut("rule/{ruleId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateRule(Guid ruleId, [FromBody] UpdateDegreeRuleRequest request, CancellationToken ct)
    {
        try
        {
            var rule = await _degreeAudit.UpdateRuleAsync(ruleId, request, ct);
            return Ok(rule);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ── DELETE /api/v1/degree-audit/rule/{ruleId} ─────────────────────────────

    // Final-Touches Phase 17 Stage 17.2 — SuperAdmin deletes a degree rule
    /// <summary>Deletes (soft) a degree rule (SuperAdmin).</summary>
    [HttpDelete("rule/{ruleId:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteRule(Guid ruleId, CancellationToken ct)
    {
        try
        {
            await _degreeAudit.DeleteRuleAsync(ruleId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // ── PUT /api/v1/degree-audit/course/{courseId}/type ──────────────────────

    // Final-Touches Phase 17 Stage 17.3 — Admin/SuperAdmin tags a course as Core or Elective
    /// <summary>Sets the CourseType (Core or Elective) on a course (Admin/SuperAdmin).</summary>
    [HttpPut("course/{courseId:guid}/type")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> SetCourseType(Guid courseId, [FromBody] SetCourseTypeRequest request, CancellationToken ct)
    {
        try
        {
            await _degreeAudit.SetCourseTypeAsync(courseId, request.CourseType, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // ── Helper ─────────────────────────────────────────────────────────────────

    private Guid GetUserId()
    {
        var raw = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
