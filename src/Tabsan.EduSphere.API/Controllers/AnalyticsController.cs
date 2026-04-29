using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Exposes analytics reports and PDF/Excel export endpoints.
/// All read endpoints require Admin or Faculty. Export endpoints require Admin or above.
/// </summary>
[ApiController]
[Route("api/analytics")]
[Authorize(Policy = "Faculty")]
public sealed class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analytics;

    /// <summary>Initialises the controller with the analytics service.</summary>
    public AnalyticsController(IAnalyticsService analytics) => _analytics = analytics;

    // ── Report endpoints ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns the performance report for a specific department or all departments.
    /// Admins see all; Faculty see their own department only.
    /// </summary>
    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformance(
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var effectiveDeptId = ResolveEffectiveDepartment(departmentId);
        var result = await _analytics.GetPerformanceReportAsync(effectiveDeptId, ct);
        return result is null ? NotFound("No data found.") : Ok(result);
    }

    /// <summary>Returns the attendance summary for a department or all departments.</summary>
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance(
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var effectiveDeptId = ResolveEffectiveDepartment(departmentId);
        var result = await _analytics.GetAttendanceReportAsync(effectiveDeptId, ct);
        return result is null ? NotFound("No data found.") : Ok(result);
    }

    /// <summary>Returns assignment statistics for a department or all departments.</summary>
    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignmentStats(
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var effectiveDeptId = ResolveEffectiveDepartment(departmentId);
        var result = await _analytics.GetAssignmentStatsAsync(effectiveDeptId, ct);
        return result is null ? NotFound("No data found.") : Ok(result);
    }

    /// <summary>Returns quiz statistics for a department or all departments.</summary>
    [HttpGet("quizzes")]
    public async Task<IActionResult> GetQuizStats(
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var effectiveDeptId = ResolveEffectiveDepartment(departmentId);
        var result = await _analytics.GetQuizStatsAsync(effectiveDeptId, ct);
        return result is null ? NotFound("No data found.") : Ok(result);
    }

    // ── Export endpoints ──────────────────────────────────────────────────────

    /// <summary>Downloads the performance report as a PDF file.</summary>
    [HttpGet("performance/export/pdf")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExportPerformancePdf(
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var bytes = await _analytics.ExportPerformancePdfAsync(departmentId, ct);
        return File(bytes, "application/pdf", "performance-report.pdf");
    }

    /// <summary>Downloads the performance report as an Excel file.</summary>
    [HttpGet("performance/export/excel")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExportPerformanceExcel(
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var bytes = await _analytics.ExportPerformanceExcelAsync(departmentId, ct);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "performance-report.xlsx");
    }

    /// <summary>Downloads the attendance report as a PDF file.</summary>
    [HttpGet("attendance/export/pdf")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExportAttendancePdf(
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var bytes = await _analytics.ExportAttendancePdfAsync(departmentId, ct);
        return File(bytes, "application/pdf", "attendance-report.pdf");
    }

    /// <summary>Downloads the attendance report as an Excel file.</summary>
    [HttpGet("attendance/export/excel")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExportAttendanceExcelAsync(
        [FromQuery] Guid? departmentId,
        CancellationToken ct)
    {
        var bytes = await _analytics.ExportAttendanceExcelAsync(departmentId, ct);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "attendance-report.xlsx");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Faculty users are scoped to their own department; Admin/SuperAdmin may query any department.
    /// Returns the requested departmentId for Admin/SuperAdmin, or the caller's own department for Faculty.
    /// </summary>
    private Guid? ResolveEffectiveDepartment(Guid? requested)
    {
        var isAdminOrAbove = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (isAdminOrAbove) return requested;

        // Faculty: always scoped to their own department.
        var claim = User.FindFirstValue("departmentId");
        return Guid.TryParse(claim, out var id) ? id : requested;
    }
}
