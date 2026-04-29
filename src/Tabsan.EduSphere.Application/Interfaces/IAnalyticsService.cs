using Tabsan.EduSphere.Application.DTOs.Analytics;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application service for analytics reports and export.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>Returns a performance report for a department, or all departments if null.</summary>
    Task<DepartmentPerformanceReport?> GetPerformanceReportAsync(Guid? departmentId, CancellationToken ct = default);

    /// <summary>Returns an attendance summary report for a department, or all departments if null.</summary>
    Task<DepartmentAttendanceReport?> GetAttendanceReportAsync(Guid? departmentId, CancellationToken ct = default);

    /// <summary>Returns assignment statistics for a department, or all departments if null.</summary>
    Task<AssignmentStatsReport?> GetAssignmentStatsAsync(Guid? departmentId, CancellationToken ct = default);

    /// <summary>Returns quiz statistics for a department, or all departments if null.</summary>
    Task<QuizStatsReport?> GetQuizStatsAsync(Guid? departmentId, CancellationToken ct = default);

    /// <summary>Exports the performance report for a department to a PDF byte array.</summary>
    Task<byte[]> ExportPerformancePdfAsync(Guid? departmentId, CancellationToken ct = default);

    /// <summary>Exports the performance report for a department to an Excel byte array.</summary>
    Task<byte[]> ExportPerformanceExcelAsync(Guid? departmentId, CancellationToken ct = default);

    /// <summary>Exports the attendance report for a department to a PDF byte array.</summary>
    Task<byte[]> ExportAttendancePdfAsync(Guid? departmentId, CancellationToken ct = default);

    /// <summary>Exports the attendance report for a department to an Excel byte array.</summary>
    Task<byte[]> ExportAttendanceExcelAsync(Guid? departmentId, CancellationToken ct = default);
}
