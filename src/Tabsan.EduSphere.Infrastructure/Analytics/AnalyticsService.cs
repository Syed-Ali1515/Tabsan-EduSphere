using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text.Json;
using Tabsan.EduSphere.Application.DTOs.Analytics;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Attendance;
using Tabsan.EduSphere.Domain.Assignments;
using Tabsan.EduSphere.Domain.Quizzes;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Analytics;

/// <summary>
/// Computes analytics reports from the database and exports them to PDF / Excel.
/// </summary>
public sealed class AnalyticsService : IAnalyticsService
{
    private readonly ApplicationDbContext _db;
    private readonly IDistributedCache _distributedCache;

    // Final-Touches Phase 34 Stage 4.1 — short-TTL distributed cache policy for expensive analytics read endpoints.
    private static readonly TimeSpan AnalyticsCacheTtl = TimeSpan.FromSeconds(30);

    /// <summary>Initialises the service with the application DbContext.</summary>
    public AnalyticsService(ApplicationDbContext db, IDistributedCache distributedCache)
    {
        _db = db;
        _distributedCache = distributedCache;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    // Performance report
    /// <summary>Returns a performance report for a department, or all departments if null.</summary>
    public async Task<DepartmentPerformanceReport?> GetPerformanceReportAsync(
        Guid? departmentId, CancellationToken ct = default)
    {
        // Final-Touches Phase 34 Stage 4.1 — cache expensive analytics report reads in shared distributed cache.
        var cacheKey = BuildAnalyticsCacheKey("performance", departmentId);
        var cached = await _distributedCache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var cachedReport = JsonSerializer.Deserialize<DepartmentPerformanceReport>(cached);
            if (cachedReport is not null)
            {
                return cachedReport;
            }
        }

        var deptName = await ResolveDeptNameAsync(departmentId, ct);
        var query =
            from sp in _db.StudentProfiles
            join u  in _db.Users           on sp.UserId           equals u.Id
            join e  in _db.Enrollments     on sp.Id               equals e.StudentProfileId
            join co in _db.CourseOfferings on e.CourseOfferingId  equals co.Id
            join c  in _db.Courses         on co.CourseId         equals c.Id
            where departmentId == null || c.DepartmentId == departmentId
            select new { sp.Id, sp.RegistrationNumber, DisplayName = u.Username, sp.CurrentSemesterNumber, OfferingId = co.Id };

        var raw = await query.Distinct().ToListAsync(ct);
        if (!raw.Any()) return null;

        var students = new List<StudentPerformanceRow>();
        foreach (var g in raw.GroupBy(r => r.Id))
        {
            var first = g.First();
            var oids  = g.Select(x => x.OfferingId).Distinct().ToList();
            var results = await _db.Results
                .Where(r => r.StudentProfileId == first.Id && oids.Contains(r.CourseOfferingId))
                .ToListAsync(ct);
            var subs = await _db.AssignmentSubmissions.CountAsync(s => s.StudentProfileId == first.Id, ct);
            var avg  = results.Any() ? results.Average(r => (double)r.MarksObtained) : 0.0;
            students.Add(new StudentPerformanceRow(first.Id, first.RegistrationNumber, first.DisplayName,
                deptName, first.CurrentSemesterNumber, Math.Round(avg, 2), subs, subs));
        }
        students = students.OrderByDescending(s => s.AverageMarks).ToList();
        var report = new DepartmentPerformanceReport(departmentId ?? Guid.Empty, deptName,
            students.Any() ? Math.Round(students.Average(s => s.AverageMarks), 2) : 0,
            students.Count, students);

        await _distributedCache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(report),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AnalyticsCacheTtl
            },
            ct);

        return report;
    }

    // Attendance report
    /// <summary>Returns an attendance summary for a department, or all departments if null.</summary>
    public async Task<DepartmentAttendanceReport?> GetAttendanceReportAsync(
        Guid? departmentId, CancellationToken ct = default)
    {
        // Final-Touches Phase 34 Stage 4.1 — cache expensive analytics report reads in shared distributed cache.
        var cacheKey = BuildAnalyticsCacheKey("attendance", departmentId);
        var cached = await _distributedCache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var cachedReport = JsonSerializer.Deserialize<DepartmentAttendanceReport>(cached);
            if (cachedReport is not null)
            {
                return cachedReport;
            }
        }

        var deptName = await ResolveDeptNameAsync(departmentId, ct);
        var rows = await (
            from ar in _db.AttendanceRecords
            join sp in _db.StudentProfiles on ar.StudentProfileId equals sp.Id
            join u  in _db.Users           on sp.UserId           equals u.Id
            join co in _db.CourseOfferings on ar.CourseOfferingId equals co.Id
            join c  in _db.Courses         on co.CourseId         equals c.Id
            where departmentId == null || c.DepartmentId == departmentId
            select new { sp.Id, DisplayName = u.Username, CourseName = c.Title, ar.Status }
        ).ToListAsync(ct);

        if (!rows.Any()) return null;

        var attendanceRows = rows
            .GroupBy(r => new { r.Id, r.DisplayName, r.CourseName })
            .Select(g =>
            {
                var total    = g.Count();
                var attended = g.Count(x => x.Status == AttendanceStatus.Present || x.Status == AttendanceStatus.Late);
                return new AttendanceRow(g.Key.Id, g.Key.DisplayName, g.Key.CourseName, total, attended,
                    total > 0 ? Math.Round((double)attended / total * 100, 1) : 0);
            })
            .OrderBy(r => r.AttendancePercentage).ToList();

        var overall = attendanceRows.Any() ? Math.Round(attendanceRows.Average(r => r.AttendancePercentage), 1) : 0;
        var report = new DepartmentAttendanceReport(departmentId ?? Guid.Empty, deptName, overall, attendanceRows);

        await _distributedCache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(report),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AnalyticsCacheTtl
            },
            ct);

        return report;
    }

    // Assignment stats
    /// <summary>Returns assignment statistics for a department, or all if null.</summary>
    public async Task<AssignmentStatsReport?> GetAssignmentStatsAsync(
        Guid? departmentId, CancellationToken ct = default)
    {
        // Final-Touches Phase 34 Stage 4.1 — cache expensive analytics report reads in shared distributed cache.
        var cacheKey = BuildAnalyticsCacheKey("assignments", departmentId);
        var cached = await _distributedCache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var cachedReport = JsonSerializer.Deserialize<AssignmentStatsReport>(cached);
            if (cachedReport is not null)
            {
                return cachedReport;
            }
        }

        var deptName = await ResolveDeptNameAsync(departmentId, ct);
        var assignments = await (
            from a  in _db.Assignments
            join co in _db.CourseOfferings on a.CourseOfferingId equals co.Id
            join c  in _db.Courses         on co.CourseId         equals c.Id
            where departmentId == null || c.DepartmentId == departmentId
            select new { a.Id, a.Title, CourseName = c.Title, OfferingId = co.Id }
        ).ToListAsync(ct);

        if (!assignments.Any()) return null;

        var stats = new List<AssignmentStatsRow>();
        foreach (var a in assignments)
        {
            var subs     = await _db.AssignmentSubmissions.Where(s => s.AssignmentId == a.Id).ToListAsync(ct);
            var graded   = subs.Count(s => s.Status == SubmissionStatus.Graded);
            var avg      = graded > 0 ? subs.Where(s => s.Status == SubmissionStatus.Graded && s.MarksAwarded.HasValue)
                               .Average(s => (double)s.MarksAwarded!.Value) : 0.0;
            var enrolled = await _db.Enrollments.CountAsync(e => e.CourseOfferingId == a.OfferingId, ct);
            stats.Add(new AssignmentStatsRow(a.Id, a.Title, a.CourseName, enrolled, subs.Count, graded, Math.Round(avg, 2)));
        }
        var report = new AssignmentStatsReport(departmentId ?? Guid.Empty, deptName, stats);

        await _distributedCache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(report),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AnalyticsCacheTtl
            },
            ct);

        return report;
    }

    // Quiz stats
    /// <summary>Returns quiz statistics for a department, or all if null.</summary>
    public async Task<QuizStatsReport?> GetQuizStatsAsync(
        Guid? departmentId, CancellationToken ct = default)
    {
        // Final-Touches Phase 34 Stage 4.1 — cache expensive analytics report reads in shared distributed cache.
        var cacheKey = BuildAnalyticsCacheKey("quizzes", departmentId);
        var cached = await _distributedCache.GetStringAsync(cacheKey, ct);
        if (!string.IsNullOrWhiteSpace(cached))
        {
            var cachedReport = JsonSerializer.Deserialize<QuizStatsReport>(cached);
            if (cachedReport is not null)
            {
                return cachedReport;
            }
        }

        var deptName = await ResolveDeptNameAsync(departmentId, ct);
        var quizzes = await (
            from q  in _db.Quizzes.IgnoreQueryFilters()
            join co in _db.CourseOfferings on q.CourseOfferingId equals co.Id
            join c  in _db.Courses         on co.CourseId         equals c.Id
            where departmentId == null || c.DepartmentId == departmentId
            select new { q.Id, q.Title, CourseName = c.Title }
        ).ToListAsync(ct);

        if (!quizzes.Any()) return null;

        var stats = new List<QuizStatsRow>();
        foreach (var q in quizzes)
        {
            var attempts  = await _db.QuizAttempts.Where(a => a.QuizId == q.Id).ToListAsync(ct);
            var submitted = attempts.Where(a => a.Status == AttemptStatus.Submitted || a.Status == AttemptStatus.TimedOut).ToList();
            var avg  = submitted.Any() ? submitted.Average(a => (double)(a.TotalScore ?? 0)) : 0.0;
            var high = attempts.Any()  ? (double)(attempts.Max(a => a.TotalScore) ?? 0)       : 0.0;
            var low  = submitted.Any() ? (double)(submitted.Min(a => a.TotalScore) ?? 0)      : 0.0;
            stats.Add(new QuizStatsRow(q.Id, q.Title, q.CourseName, attempts.Count, submitted.Count,
                Math.Round(avg, 2), Math.Round(high, 2), Math.Round(low, 2)));
        }
        var report = new QuizStatsReport(departmentId ?? Guid.Empty, deptName, stats);

        await _distributedCache.SetStringAsync(
            cacheKey,
            JsonSerializer.Serialize(report),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = AnalyticsCacheTtl
            },
            ct);

        return report;
    }

    // PDF exports
    /// <summary>Exports the performance report to a PDF byte array.</summary>
    public async Task<byte[]> ExportPerformancePdfAsync(Guid? departmentId, CancellationToken ct = default)
    {
        var report = await GetPerformanceReportAsync(departmentId, ct)
                     ?? new DepartmentPerformanceReport(Guid.Empty, "All Departments", 0, 0, []);
        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.Content().Column(col =>
            {
                col.Item().Text($"Performance Report - {report.DepartmentName}").FontSize(16).Bold();
                col.Item().Text($"Overall Average: {report.AverageMarks:F1}  |  Students: {report.TotalStudents}").FontSize(10);
                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(2);
                        c.RelativeColumn(1); c.RelativeColumn(1); c.RelativeColumn(1);
                    });
                    AddPdfHeader(table, "Reg No", "Name", "Department", "Semester", "Avg Marks", "Submitted");
                    foreach (var s in report.Students)
                        AddPdfRow(table, s.RegistrationNumber, s.FullName, s.Department,
                            s.CurrentSemester.ToString(), s.AverageMarks.ToString("F1"), s.SubmittedAssignments.ToString());
                });
            });
        })).GeneratePdf();
    }

    /// <summary>Exports the attendance report to a PDF byte array.</summary>
    public async Task<byte[]> ExportAttendancePdfAsync(Guid? departmentId, CancellationToken ct = default)
    {
        var report = await GetAttendanceReportAsync(departmentId, ct)
                     ?? new DepartmentAttendanceReport(Guid.Empty, "All Departments", 0, []);
        return Document.Create(container => container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
            page.Margin(30);
            page.Content().Column(col =>
            {
                col.Item().Text($"Attendance Report - {report.DepartmentName}").FontSize(16).Bold();
                col.Item().Text($"Overall Attendance: {report.OverallAttendancePercentage:F1}%").FontSize(10);
                col.Item().PaddingTop(10).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(2); c.RelativeColumn(2); c.RelativeColumn(1);
                        c.RelativeColumn(1); c.RelativeColumn(1);
                    });
                    AddPdfHeader(table, "Name", "Course", "Total", "Attended", "Attendance %");
                    foreach (var r in report.Rows)
                        AddPdfRow(table, r.FullName, r.CourseName,
                            r.TotalClasses.ToString(), r.AttendedClasses.ToString(),
                            r.AttendancePercentage.ToString("F1") + "%");
                });
            });
        })).GeneratePdf();
    }

    // Excel exports
    /// <summary>Exports the performance report to an Excel byte array.</summary>
    public async Task<byte[]> ExportPerformanceExcelAsync(Guid? departmentId, CancellationToken ct = default)
    {
        var report = await GetPerformanceReportAsync(departmentId, ct)
                     ?? new DepartmentPerformanceReport(Guid.Empty, "All Departments", 0, 0, []);
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Performance");
        ws.Cell(1, 1).Value = $"Performance Report - {report.DepartmentName}";
        ws.Range(1, 1, 1, 6).Merge().Style.Font.Bold = true;
        string[] h1 = ["Reg No", "Name", "Department", "Semester", "Avg Marks", "Submitted"];
        for (int i = 0; i < h1.Length; i++) { ws.Cell(2, i + 1).Value = h1[i]; ws.Cell(2, i + 1).Style.Font.Bold = true; }
        int row = 3;
        foreach (var s in report.Students)
        {
            ws.Cell(row, 1).Value = s.RegistrationNumber; ws.Cell(row, 2).Value = s.FullName;
            ws.Cell(row, 3).Value = s.Department;         ws.Cell(row, 4).Value = s.CurrentSemester;
            ws.Cell(row, 5).Value = s.AverageMarks;       ws.Cell(row, 6).Value = s.SubmittedAssignments; row++;
        }
        ws.Columns().AdjustToContents();
        using var ms1 = new MemoryStream(); wb.SaveAs(ms1); return ms1.ToArray();
    }

    /// <summary>Exports the attendance report to an Excel byte array.</summary>
    public async Task<byte[]> ExportAttendanceExcelAsync(Guid? departmentId, CancellationToken ct = default)
    {
        var report = await GetAttendanceReportAsync(departmentId, ct)
                     ?? new DepartmentAttendanceReport(Guid.Empty, "All Departments", 0, []);
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Attendance");
        ws.Cell(1, 1).Value = $"Attendance Report - {report.DepartmentName}";
        ws.Range(1, 1, 1, 5).Merge().Style.Font.Bold = true;
        string[] h2 = ["Name", "Course", "Total Classes", "Attended", "Attendance %"];
        for (int i = 0; i < h2.Length; i++) { ws.Cell(2, i + 1).Value = h2[i]; ws.Cell(2, i + 1).Style.Font.Bold = true; }
        int row = 3;
        foreach (var r in report.Rows)
        {
            ws.Cell(row, 1).Value = r.FullName;          ws.Cell(row, 2).Value = r.CourseName;
            ws.Cell(row, 3).Value = r.TotalClasses;      ws.Cell(row, 4).Value = r.AttendedClasses;
            ws.Cell(row, 5).Value = r.AttendancePercentage; row++;
        }
        ws.Columns().AdjustToContents();
        using var ms2 = new MemoryStream(); wb.SaveAs(ms2); return ms2.ToArray();
    }

    // Private helpers
    /// <summary>Resolves a department name from its ID; returns "All Departments" for null.</summary>
    private async Task<string> ResolveDeptNameAsync(Guid? departmentId, CancellationToken ct)
    {
        if (departmentId is null) return "All Departments";
        var dept = await _db.Departments.FindAsync([departmentId], ct);
        return dept?.Name ?? "Unknown Department";
    }

    private static string BuildAnalyticsCacheKey(string reportType, Guid? departmentId)
    {
        var departmentSegment = departmentId?.ToString("N") ?? "all";
        return $"analytics:{reportType}:{departmentSegment}";
    }

    private static void AddPdfHeader(TableDescriptor table, params string[] headers)
    {
        foreach (var h in headers)
            table.Header(header =>
                header.Cell().Background("#2563EB").Padding(4).Text(h).FontColor("#FFFFFF").Bold());
    }

    private static void AddPdfRow(TableDescriptor table, params string[] values)
    {
        foreach (var v in values)
            table.Cell().BorderBottom(0.5f).Padding(4).Text(v).FontSize(9);
    }
}
