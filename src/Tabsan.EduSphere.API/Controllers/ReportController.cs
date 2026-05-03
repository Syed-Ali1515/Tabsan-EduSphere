using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Reports;
using Tabsan.EduSphere.Application.Interfaces;

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

    public ReportController(IReportService reports) => _reports = reports;

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
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetAttendanceSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var request = new AttendanceSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var result = await _reports.GetAttendanceSummaryAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Downloads attendance summary as an Excel (.xlsx) file.</summary>
    [HttpGet("attendance-summary/export")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> ExportAttendanceSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var request = new AttendanceSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportAttendanceSummaryExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "attendance-summary.xlsx");
    }

    // ── Result Summary ─────────────────────────────────────────────────────────

    /// <summary>Returns published result data with optional filters.</summary>
    [HttpGet("result-summary")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetResultSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var request = new ResultSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var result = await _reports.GetResultSummaryAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Downloads result summary as an Excel file.</summary>
    [HttpGet("result-summary/export")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> ExportResultSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? courseOfferingId,
        [FromQuery] Guid? studentProfileId,
        CancellationToken ct)
    {
        var request = new ResultSummaryRequest(semesterId, departmentId, courseOfferingId, studentProfileId);
        var bytes = await _reports.ExportResultSummaryExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "result-summary.xlsx");
    }

    // ── GPA Report ─────────────────────────────────────────────────────────────

    /// <summary>Returns per-student GPA and CGPA data.</summary>
    [HttpGet("gpa-report")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetGpaReport(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? programId,
        CancellationToken ct)
    {
        var request = new GpaReportRequest(departmentId, programId);
        var result = await _reports.GetGpaReportAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Downloads GPA report as an Excel file.</summary>
    [HttpGet("gpa-report/export")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> ExportGpaReport(
        [FromQuery] Guid? departmentId,
        [FromQuery] Guid? programId,
        CancellationToken ct)
    {
        var request = new GpaReportRequest(departmentId, programId);
        var bytes = await _reports.ExportGpaReportExcelAsync(request, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "gpa-report.xlsx");
    }

    // ── Enrollment Summary ─────────────────────────────────────────────────────

    /// <summary>Returns course offering enrollment utilisation data.</summary>
    [HttpGet("enrollment-summary")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetEnrollmentSummary(
        [FromQuery] Guid? semesterId,
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var request = new EnrollmentSummaryRequest(semesterId, departmentId);
        var result = await _reports.GetEnrollmentSummaryAsync(request, ct);
        return Ok(result);
    }

    // ── Semester Results ───────────────────────────────────────────────────────

    /// <summary>Returns all published results for a specific semester.</summary>
    [HttpGet("semester-results")]
    [Authorize(Policy = "Faculty")]
    public async Task<IActionResult> GetSemesterResults(
        [FromQuery] Guid semesterId,
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        if (semesterId == Guid.Empty)
            return BadRequest("semesterId is required.");

        var request = new SemesterResultsRequest(semesterId, departmentId);
        var result = await _reports.GetSemesterResultsAsync(request, ct);
        return Ok(result);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private string? GetCurrentUserRole() =>
        User.FindFirstValue(ClaimTypes.Role);
}
