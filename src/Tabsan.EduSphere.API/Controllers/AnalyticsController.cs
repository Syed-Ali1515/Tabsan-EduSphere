using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

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
    private readonly IDepartmentRepository _departments;

    /// <summary>Initialises the controller with the analytics service.</summary>
    public AnalyticsController(IAnalyticsService analytics, IDepartmentRepository departments)
    {
        _analytics = analytics;
        _departments = departments;
    }

    // ── Report endpoints ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns the performance report for a specific department or all departments.
    /// Admins see all; Faculty see their own department only.
    /// </summary>
    [HttpGet("performance")]
    public async Task<IActionResult> GetPerformance(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? institutionType,
        CancellationToken ct)
    {
        var scope = await ResolveEffectiveScopeAsync(departmentId, institutionType, ct);
        if (scope.Error is not null) return scope.Error;

        var result = await _analytics.GetPerformanceReportAsync(scope.DepartmentId, scope.InstitutionType, ct);
        return result is null ? NotFound("No data found.") : Ok(result);
    }

    /// <summary>Returns the attendance summary for a department or all departments.</summary>
    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? institutionType,
        CancellationToken ct)
    {
        var scope = await ResolveEffectiveScopeAsync(departmentId, institutionType, ct);
        if (scope.Error is not null) return scope.Error;

        var result = await _analytics.GetAttendanceReportAsync(scope.DepartmentId, scope.InstitutionType, ct);
        return result is null ? NotFound("No data found.") : Ok(result);
    }

    /// <summary>Returns assignment statistics for a department or all departments.</summary>
    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignmentStats(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? institutionType,
        CancellationToken ct)
    {
        var scope = await ResolveEffectiveScopeAsync(departmentId, institutionType, ct);
        if (scope.Error is not null) return scope.Error;

        var result = await _analytics.GetAssignmentStatsAsync(scope.DepartmentId, scope.InstitutionType, ct);
        return result is null ? NotFound("No data found.") : Ok(result);
    }

    /// <summary>Returns quiz statistics for a department or all departments.</summary>
    [HttpGet("quizzes")]
    public async Task<IActionResult> GetQuizStats(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? institutionType,
        CancellationToken ct)
    {
        var scope = await ResolveEffectiveScopeAsync(departmentId, institutionType, ct);
        if (scope.Error is not null) return scope.Error;

        var result = await _analytics.GetQuizStatsAsync(scope.DepartmentId, scope.InstitutionType, ct);
        return result is null ? NotFound("No data found.") : Ok(result);
    }

    // ── Export endpoints ──────────────────────────────────────────────────────

    /// <summary>Downloads the performance report as a PDF file.</summary>
    [HttpGet("performance/export/pdf")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExportPerformancePdf(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? institutionType,
        CancellationToken ct)
    {
        var scope = await ResolveEffectiveScopeAsync(departmentId, institutionType, ct);
        if (scope.Error is not null) return scope.Error;

        var bytes = await _analytics.ExportPerformancePdfAsync(scope.DepartmentId, scope.InstitutionType, ct);
        return File(bytes, "application/pdf", "performance-report.pdf");
    }

    /// <summary>Downloads the performance report as an Excel file.</summary>
    [HttpGet("performance/export/excel")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExportPerformanceExcel(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? institutionType,
        CancellationToken ct)
    {
        var scope = await ResolveEffectiveScopeAsync(departmentId, institutionType, ct);
        if (scope.Error is not null) return scope.Error;

        var bytes = await _analytics.ExportPerformanceExcelAsync(scope.DepartmentId, scope.InstitutionType, ct);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "performance-report.xlsx");
    }

    /// <summary>Downloads the attendance report as a PDF file.</summary>
    [HttpGet("attendance/export/pdf")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExportAttendancePdf(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? institutionType,
        CancellationToken ct)
    {
        var scope = await ResolveEffectiveScopeAsync(departmentId, institutionType, ct);
        if (scope.Error is not null) return scope.Error;

        var bytes = await _analytics.ExportAttendancePdfAsync(scope.DepartmentId, scope.InstitutionType, ct);
        return File(bytes, "application/pdf", "attendance-report.pdf");
    }

    /// <summary>Downloads the attendance report as an Excel file.</summary>
    [HttpGet("attendance/export/excel")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ExportAttendanceExcelAsync(
        [FromQuery] Guid? departmentId,
        [FromQuery] int? institutionType,
        CancellationToken ct)
    {
        var scope = await ResolveEffectiveScopeAsync(departmentId, institutionType, ct);
        if (scope.Error is not null) return scope.Error;

        var bytes = await _analytics.ExportAttendanceExcelAsync(scope.DepartmentId, scope.InstitutionType, ct);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "attendance-report.xlsx");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private int? GetCurrentInstitutionType()
    {
        var raw = User.FindFirst("institutionType")?.Value;
        return int.TryParse(raw, out var value) ? value : null;
    }

    /// <summary>
    /// Faculty users are scoped to their own department. Constrained roles are auto-scoped
    /// to their institution claim when present; explicit mismatches are forbidden.
    /// </summary>
    private async Task<(Guid? DepartmentId, int? InstitutionType, IActionResult? Error)> ResolveEffectiveScopeAsync(
        Guid? requestedDepartmentId,
        int? requestedInstitutionType,
        CancellationToken ct)
    {
        Guid? effectiveDepartmentId = requestedDepartmentId;

        var isAdminOrAbove = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");
        if (!isAdminOrAbove)
        {
            var claim = User.FindFirstValue("departmentId");
            if (Guid.TryParse(claim, out var facultyDepartmentId))
                effectiveDepartmentId = facultyDepartmentId;
        }

        var callerInstitutionType = GetCurrentInstitutionType();
        var effectiveInstitutionType = requestedInstitutionType;

        if (!User.IsInRole("SuperAdmin") && callerInstitutionType.HasValue)
        {
            if (requestedInstitutionType.HasValue && requestedInstitutionType.Value != callerInstitutionType.Value)
                return (null, null, Forbid());

            effectiveInstitutionType = callerInstitutionType.Value;
        }

        if (effectiveDepartmentId.HasValue)
        {
            var department = await _departments.GetByIdAsync(effectiveDepartmentId.Value, ct);
            if (department is null)
                return (null, null, NotFound("Department not found."));

            if (effectiveInstitutionType.HasValue && (int)department.InstitutionType != effectiveInstitutionType.Value)
                return (null, null, Forbid());

            if (!effectiveInstitutionType.HasValue)
                effectiveInstitutionType = (int)department.InstitutionType;
        }

        return (effectiveDepartmentId, effectiveInstitutionType, null);
    }
}
