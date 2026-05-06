using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Reports;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Role-gated report data and Excel export endpoints for Phase 12 Reporting.
/// Catalog is accessible to all authenticated roles.
/// Data and export endpoints require Admin or Faculty.
/// </summary>
[ApiController]
[Route("api/v1/reports")]
[Authorize]
public sealed class ReportController : ControllerBase
{
    private readonly IReportService _reports;
    private readonly ICourseRepository _courses;
    private readonly IAdminAssignmentRepository _adminAssignments;

    public ReportController(IReportService reports, ICourseRepository courses, IAdminAssignmentRepository adminAssignments)
    {
        _reports = reports;
        _courses = courses;
        _adminAssignments = adminAssignments;
    }

    // ── Catalog ────────────────────────────────────────────────────────────────

    /// <summary>Returns all active reports the caller's role is permitted to view.</summary>
    [HttpGet]
    public async Task<IActionResult> GetCatalog(CancellationToken ct)
    {
        var role = GetCurrentUserRole();
        if (role is null) return Unauthorized();
        var result = await _reports.GetCatalogAsync(role, ct);
        return Ok(result);
    }

    // ── Attendance Summary ─────────────────────────────────────────────────────

    /// <summary>Returns attendance summary data with optional filters.</summary>
    [HttpGet("attendance-summary")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetAttendanceSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new AttendanceSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var result = await _reports.GetAttendanceSummaryAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Downloads attendance summary as an Excel (.xlsx) file.</summary>
    [HttpGet("attendance-summary/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportAttendanceSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new AttendanceSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportAttendanceSummaryExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "attendance-summary.xlsx");
    }

    /// <summary>Downloads attendance summary as CSV.</summary>
    [HttpGet("attendance-summary/export/csv")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportAttendanceSummaryCsv(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new AttendanceSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportAttendanceSummaryCsvAsync(request, ct);
        return File(bytes, "text/csv", "attendance-summary.csv");
    }

    /// <summary>Downloads attendance summary as PDF.</summary>
    [HttpGet("attendance-summary/export/pdf")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportAttendanceSummaryPdf(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new AttendanceSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportAttendanceSummaryPdfAsync(request, ct);
        return File(bytes, "application/pdf", "attendance-summary.pdf");
    }

    // ── Result Summary ─────────────────────────────────────────────────────────

    /// <summary>Returns published result data with optional filters.</summary>
    [HttpGet("result-summary")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetResultSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new ResultSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var result = await _reports.GetResultSummaryAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Downloads result summary as an Excel file.</summary>
    [HttpGet("result-summary/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportResultSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new ResultSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportResultSummaryExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "result-summary.xlsx");
    }

    /// <summary>Downloads result summary as CSV.</summary>
    [HttpGet("result-summary/export/csv")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportResultSummaryCsv(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new ResultSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportResultSummaryCsvAsync(request, ct);
        return File(bytes, "text/csv", "result-summary.csv");
    }

    /// <summary>Downloads result summary as PDF.</summary>
    [HttpGet("result-summary/export/pdf")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportResultSummaryPdf(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new ResultSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportResultSummaryPdfAsync(request, ct);
        return File(bytes, "application/pdf", "result-summary.pdf");
    }

    // ── Assignment Summary ───────────────────────────────────────────────────

    /// <summary>Returns assignment submission data with optional filters.</summary>
    [HttpGet("assignment-summary")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetAssignmentSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new AssignmentSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var result = await _reports.GetAssignmentSummaryAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Downloads assignment summary as an Excel file.</summary>
    [HttpGet("assignment-summary/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportAssignmentSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new AssignmentSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportAssignmentSummaryExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "assignment-summary.xlsx");
    }

    /// <summary>Downloads assignment summary as CSV.</summary>
    [HttpGet("assignment-summary/export/csv")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportAssignmentSummaryCsv(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new AssignmentSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportAssignmentSummaryCsvAsync(request, ct);
        return File(bytes, "text/csv", "assignment-summary.csv");
    }

    /// <summary>Downloads assignment summary as PDF.</summary>
    [HttpGet("assignment-summary/export/pdf")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportAssignmentSummaryPdf(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new AssignmentSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportAssignmentSummaryPdfAsync(request, ct);
        return File(bytes, "application/pdf", "assignment-summary.pdf");
    }

    // ── Quiz Summary ─────────────────────────────────────────────────────────

    /// <summary>Returns quiz attempt data with optional filters.</summary>
    [HttpGet("quiz-summary")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetQuizSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new QuizSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var result = await _reports.GetQuizSummaryAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Downloads quiz summary as an Excel file.</summary>
    [HttpGet("quiz-summary/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportQuizSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new QuizSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportQuizSummaryExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "quiz-summary.xlsx");
    }

    /// <summary>Downloads quiz summary as CSV.</summary>
    [HttpGet("quiz-summary/export/csv")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportQuizSummaryCsv(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new QuizSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportQuizSummaryCsvAsync(request, ct);
        return File(bytes, "text/csv", "quiz-summary.csv");
    }

    /// <summary>Downloads quiz summary as PDF.</summary>
    [HttpGet("quiz-summary/export/pdf")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportQuizSummaryPdf(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        scoped = await EnforceFacultyOfferingScopeAsync(courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new QuizSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportQuizSummaryPdfAsync(request, ct);
        return File(bytes, "application/pdf", "quiz-summary.pdf");
    }

    // ── GPA Report ─────────────────────────────────────────────────────────────

    /// <summary>Returns per-student GPA and CGPA data.</summary>
    [HttpGet("gpa-report")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetGpaReport(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? programId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, null, ct);
        if (scoped is not null) return scoped;

        var request = new GpaReportRequest(departmentId, programId);
        var result = await _reports.GetGpaReportAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Downloads GPA report as an Excel file.</summary>
    [HttpGet("gpa-report/export")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> ExportGpaReport(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? programId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, null, ct);
        if (scoped is not null) return scoped;

        var request = new GpaReportRequest(departmentId, programId);
        var bytes = await _reports.ExportGpaReportExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "gpa-report.xlsx");
    }

    // ── Enrollment Summary ─────────────────────────────────────────────────────

    /// <summary>Returns course offering enrollment utilisation data.</summary>
    [HttpGet("enrollment-summary")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetEnrollmentSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, null, ct);
        if (scoped is not null) return scoped;

        var request = new EnrollmentSummaryRequest(semesterId, departmentId);
        var result = await _reports.GetEnrollmentSummaryAsync(request, ct);
        return Ok(result);
    }

    // ── Semester Results ───────────────────────────────────────────────────────

    /// <summary>Returns all published results for a specific semester.</summary>
    [HttpGet("semester-results")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetSemesterResults(
        [FromQuery] Guid semesterId,
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        if (semesterId == Guid.Empty)
            return BadRequest("semesterId is required.");

        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, null, ct);
        if (scoped is not null) return scoped;

        var request = new SemesterResultsRequest(semesterId, departmentId);
        var result = await _reports.GetSemesterResultsAsync(request, ct);
        return Ok(result);
    }

    // ── Stage 4.2: Student Transcript ─────────────────────────────────────────

    /// <summary>Returns all published results for a single student (transcript).</summary>
    [HttpGet("student-transcript")]
    [Authorize]
    public async Task<IActionResult> GetStudentTranscript(
        [FromQuery] Guid studentProfileId,
        CancellationToken ct)
    {
        if (studentProfileId == Guid.Empty)
            return BadRequest("studentProfileId is required.");

        var request = new TranscriptRequest(studentProfileId);
        var result = await _reports.GetStudentTranscriptAsync(request, ct);
        if (result is null) return NotFound("Student not found.");
        return Ok(result);
    }

    /// <summary>Downloads student transcript as an Excel file.</summary>
    [HttpGet("student-transcript/export")]
    [Authorize]
    public async Task<IActionResult> ExportStudentTranscript(
        [FromQuery] Guid studentProfileId,
        CancellationToken ct)
    {
        if (studentProfileId == Guid.Empty)
            return BadRequest("studentProfileId is required.");

        var request = new TranscriptRequest(studentProfileId);
        var bytes = await _reports.ExportTranscriptExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "student-transcript.xlsx");
    }

    // ── Stage 4.2: Low Attendance Warning ─────────────────────────────────────

    /// <summary>Returns students whose attendance is below the given threshold.</summary>
    [HttpGet("low-attendance")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetLowAttendanceWarning(
        [FromQuery] decimal threshold = 75m,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? courseOfferingId = null,
        CancellationToken ct = default)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, courseOfferingId, ct);
        if (scoped is not null) return scoped;

        var request = new LowAttendanceRequest(threshold, departmentId, courseOfferingId);
        var result = await _reports.GetLowAttendanceWarningAsync(request, ct);
        return Ok(result);
    }

    // ── Stage 4.2: FYP Status Report ──────────────────────────────────────────

    /// <summary>Returns all FYP project rows, optionally filtered by department and status.</summary>
    [HttpGet("fyp-status")]
    [Authorize(Roles = "SuperAdmin,Admin,Faculty")]
    public async Task<IActionResult> GetFypStatusReport(
        [FromQuery] Guid? departmentId,
        [FromQuery] string? status,
        CancellationToken ct)
    {
        var scoped = await EnforceAdminDepartmentScopeAsync(departmentId, null, ct);
        if (scoped is not null) return scoped;

        var request = new FypStatusRequest(departmentId, status);
        var result = await _reports.GetFypStatusReportAsync(request, ct);
        return Ok(result);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private string? GetCurrentUserRole() =>
        User.FindFirstValue(ClaimTypes.Role);

    private Guid GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }

    private async Task<IActionResult?> EnforceFacultyOfferingScopeAsync(Guid? courseOfferingId, CancellationToken ct)
    {
        if (!User.IsInRole("Faculty") || User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            return null;

        if (!courseOfferingId.HasValue || courseOfferingId.Value == Guid.Empty)
            return BadRequest("Faculty must select a course offering for report generation.");

        var offering = await _courses.GetOfferingByIdAsync(courseOfferingId.Value, ct);
        if (offering is null) return NotFound("Course offering not found.");

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty || offering.FacultyUserId != userId)
            return Forbid();

        return null;
    }

    private async Task<IActionResult?> EnforceAdminDepartmentScopeAsync(Guid? departmentId, Guid? courseOfferingId, CancellationToken ct)
    {
        if (!User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
            return null;

        var adminUserId = GetCurrentUserId();
        if (adminUserId == Guid.Empty)
            return Forbid();

        var allowedDepartmentIds = await _adminAssignments.GetDepartmentIdsForAdminAsync(adminUserId, ct);
        if (allowedDepartmentIds.Count == 0)
            return Forbid();

        if (departmentId.HasValue && !allowedDepartmentIds.Contains(departmentId.Value))
            return Forbid();

        if (courseOfferingId.HasValue && courseOfferingId.Value != Guid.Empty)
        {
            var offering = await _courses.GetOfferingByIdAsync(courseOfferingId.Value, ct);
            if (offering is null)
                return NotFound("Course offering not found.");

            if (!allowedDepartmentIds.Contains(offering.Course.DepartmentId))
                return Forbid();
        }

        // Multi-department admin scope requires at least one explicit filter to avoid cross-dept aggregate leakage.
        if (!departmentId.HasValue && !courseOfferingId.HasValue)
            return BadRequest("Admin must select a department or course offering for report generation.");

        return null;
    }
}
