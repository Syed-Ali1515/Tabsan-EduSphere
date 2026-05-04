using ClosedXML.Excel;
using Tabsan.EduSphere.Application.DTOs.Reports;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Reporting;

/// <summary>
/// Generates report data and Excel exports for Phase 12 Reporting.
/// Lives in Infrastructure to access ClosedXML.
/// </summary>
public sealed class ReportService : IReportService
{
    private readonly IReportRepository _repo;

    public ReportService(IReportRepository repo) => _repo = repo;

    // ── Catalog ────────────────────────────────────────────────────────────────

    public async Task<ReportCatalogResponse> GetCatalogAsync(string roleName, CancellationToken ct = default)
    {
        var defs = await _repo.GetCatalogForRoleAsync(roleName, ct);
        var items = defs.Select(d => new ReportCatalogItemResponse(
            d.Id,
            d.Key,
            d.Name,
            d.Purpose,
            d.IsActive,
            d.RoleAssignments.Select(ra => ra.RoleName).ToList()
        )).ToList();
        return new ReportCatalogResponse(items);
    }

    // ── Attendance Summary ─────────────────────────────────────────────────────

    public async Task<AttendanceSummaryReportResponse> GetAttendanceSummaryAsync(
        AttendanceSummaryRequest request, CancellationToken ct = default)
    {
        var raw = await _repo.GetAttendanceDataAsync(
            request.SemesterId, request.CourseOfferingId, request.StudentProfileId, ct);

        var rows = raw.Select(r => new AttendanceSummaryRow(
            r.StudentProfileId, r.RegistrationNumber, r.StudentName,
            r.CourseOfferingId, r.CourseCode, r.CourseTitle,
            r.TotalSessions, r.AttendedSessions, r.AttendancePercentage)).ToList();

        return new AttendanceSummaryReportResponse(
            rows,
            rows.Select(r => r.StudentProfileId).Distinct().Count(),
            DateTime.UtcNow);
    }

    // ── Result Summary ─────────────────────────────────────────────────────────

    public async Task<ResultSummaryReportResponse> GetResultSummaryAsync(
        ResultSummaryRequest request, CancellationToken ct = default)
    {
        var raw = await _repo.GetResultDataAsync(
            request.SemesterId, request.CourseOfferingId, request.StudentProfileId, ct);

        var rows = raw.Select(r => new ResultSummaryRow(
            r.StudentProfileId, r.RegistrationNumber, r.StudentName,
            r.CourseCode, r.CourseTitle, r.ResultType,
            r.MarksObtained, r.MaxMarks, r.Percentage, r.PublishedAt)).ToList();

        return new ResultSummaryReportResponse(rows, rows.Count, DateTime.UtcNow);
    }

    // ── GPA Report ─────────────────────────────────────────────────────────────

    public async Task<GpaReportResponse> GetGpaReportAsync(
        GpaReportRequest request, CancellationToken ct = default)
    {
        var raw = await _repo.GetGpaDataAsync(request.DepartmentId, request.ProgramId, ct);

        var rows = raw.Select(r => new Tabsan.EduSphere.Application.DTOs.Reports.GpaReportRow(
            r.StudentProfileId, r.RegistrationNumber, r.StudentName,
            r.ProgramName, r.DepartmentName,
            r.CurrentSemesterNumber, r.Cgpa, r.CurrentSemesterGpa)).ToList();

        var avgCgpa = rows.Any() ? Math.Round(rows.Average(r => r.Cgpa), 2) : 0m;
        return new GpaReportResponse(rows, avgCgpa, rows.Count, DateTime.UtcNow);
    }

    // ── Enrollment Summary ─────────────────────────────────────────────────────

    public async Task<EnrollmentSummaryReportResponse> GetEnrollmentSummaryAsync(
        EnrollmentSummaryRequest request, CancellationToken ct = default)
    {
        var raw = await _repo.GetEnrollmentDataAsync(request.SemesterId, request.DepartmentId, ct);

        var rows = raw.Select(r => new EnrollmentSummaryRow(
            r.CourseOfferingId, r.CourseCode, r.CourseTitle, r.SemesterName,
            r.MaxEnrollment, r.EnrolledCount,
            r.MaxEnrollment - r.EnrolledCount)).ToList();

        return new EnrollmentSummaryReportResponse(rows, rows.Count, DateTime.UtcNow);
    }

    // ── Semester Results ───────────────────────────────────────────────────────

    public async Task<SemesterResultsReportResponse> GetSemesterResultsAsync(
        SemesterResultsRequest request, CancellationToken ct = default)
    {
        var raw = await _repo.GetSemesterResultDataAsync(request.SemesterId, request.DepartmentId, ct);

        var rows = raw.Select(r => new SemesterResultsRow(
            r.StudentProfileId, r.RegistrationNumber, r.StudentName,
            r.CourseCode, r.CourseTitle, r.ResultType,
            r.MarksObtained, r.MaxMarks, r.Percentage)).ToList();

        return new SemesterResultsReportResponse(
            rows,
            rows.Select(r => r.StudentProfileId).Distinct().Count(),
            DateTime.UtcNow);
    }

    // ── Excel Exports ──────────────────────────────────────────────────────────

    public async Task<byte[]> ExportAttendanceSummaryExcelAsync(
        AttendanceSummaryRequest request, CancellationToken ct = default)
    {
        var report = await GetAttendanceSummaryAsync(request, ct);
        var headers = new[] { "Reg No", "Student", "Course Code", "Course Title", "Total Sessions", "Attended", "Attendance %" };
        var rows = report.Rows.Select(r => new object[]
        {
            r.RegistrationNumber, r.StudentName, r.CourseCode, r.CourseTitle,
            r.TotalSessions, r.AttendedSessions, r.AttendancePercentage
        }).ToList();
        return BuildExcelBytes("Attendance Summary", headers, rows);
    }

    public async Task<byte[]> ExportResultSummaryExcelAsync(
        ResultSummaryRequest request, CancellationToken ct = default)
    {
        var report = await GetResultSummaryAsync(request, ct);
        var headers = new[] { "Reg No", "Student", "Course Code", "Course Title", "Component", "Marks", "Max Marks", "Percentage", "Published" };
        var rows = report.Rows.Select(r => new object[]
        {
            r.RegistrationNumber, r.StudentName, r.CourseCode, r.CourseTitle, r.ResultType,
            r.MarksObtained, r.MaxMarks, r.Percentage,
            r.PublishedAt.HasValue ? r.PublishedAt.Value.ToString("yyyy-MM-dd") : "-"
        }).ToList();
        return BuildExcelBytes("Result Summary", headers, rows);
    }

    public async Task<byte[]> ExportGpaReportExcelAsync(
        GpaReportRequest request, CancellationToken ct = default)
    {
        var report = await GetGpaReportAsync(request, ct);
        var headers = new[] { "Reg No", "Student", "Program", "Department", "Semester", "CGPA", "Semester GPA" };
        var rows = report.Rows.Select(r => new object[]
        {
            r.RegistrationNumber, r.StudentName, r.ProgramName, r.DepartmentName,
            r.CurrentSemester, r.Cgpa, r.CurrentSemesterGpa
        }).ToList();
        return BuildExcelBytes("GPA Report", headers, rows);
    }

    // ── Stage 4.2: Student Transcript ─────────────────────────────────────────

    public async Task<TranscriptReportResponse?> GetStudentTranscriptAsync(
        TranscriptRequest request, CancellationToken ct = default)
    {
        var raw = await _repo.GetTranscriptDataAsync(request.StudentProfileId, ct);
        if (raw is null) return null;

        var rows = raw.Rows.Select(r => new TranscriptRow(
            r.CourseCode, r.CourseTitle, r.SemesterName, r.ResultType,
            r.MarksObtained, r.MaxMarks, r.Percentage, r.GradePoint, r.PublishedAt)).ToList();

        return new TranscriptReportResponse(
            raw.StudentProfileId, raw.RegistrationNumber, raw.StudentName,
            raw.ProgramName, raw.DepartmentName, raw.Cgpa,
            rows, DateTime.UtcNow);
    }

    public async Task<byte[]> ExportTranscriptExcelAsync(
        TranscriptRequest request, CancellationToken ct = default)
    {
        var report = await GetStudentTranscriptAsync(request, ct);
        if (report is null) return Array.Empty<byte>();

        var headers = new[] { "Course Code", "Course Title", "Semester", "Component", "Marks", "Max Marks", "%", "Grade Point", "Published" };
        var rows = report.Rows.Select(r => new object[]
        {
            r.CourseCode, r.CourseTitle, r.SemesterName, r.ResultType,
            r.MarksObtained, r.MaxMarks, r.Percentage,
            r.GradePoint.HasValue ? (object)r.GradePoint.Value : "-",
            r.PublishedAt.HasValue ? r.PublishedAt.Value.ToString("yyyy-MM-dd") : "-"
        }).ToList();
        return BuildExcelBytes($"Transcript_{report.RegistrationNumber}", headers, rows);
    }

    // ── Stage 4.2: Low Attendance Warning ─────────────────────────────────────

    public async Task<LowAttendanceReportResponse> GetLowAttendanceWarningAsync(
        LowAttendanceRequest request, CancellationToken ct = default)
    {
        var raw = await _repo.GetLowAttendanceDataAsync(
            request.ThresholdPercent, request.DepartmentId, request.CourseOfferingId, ct);

        var rows = raw.Select(r => new LowAttendanceRow(
            r.StudentProfileId, r.RegistrationNumber, r.StudentName,
            r.CourseCode, r.CourseTitle, r.SemesterName, r.DepartmentName,
            r.TotalSessions, r.AttendedSessions, r.AttendancePercentage)).ToList();

        return new LowAttendanceReportResponse(rows, request.ThresholdPercent, rows.Select(r => r.StudentProfileId).Distinct().Count(), DateTime.UtcNow);
    }

    // ── Stage 4.2: FYP Status Report ──────────────────────────────────────────

    public async Task<FypStatusReportResponse> GetFypStatusReportAsync(
        FypStatusRequest request, CancellationToken ct = default)
    {
        var raw = await _repo.GetFypStatusDataAsync(request.DepartmentId, request.Status, ct);

        var rows = raw.Select(r => new FypStatusRow(
            r.ProjectId, r.Title, r.StudentName, r.RegistrationNumber,
            r.DepartmentName, r.SupervisorName, r.Status, r.ProposedAt, r.MeetingCount)).ToList();

        return new FypStatusReportResponse(rows, rows.Count, DateTime.UtcNow);
    }

    // ── Private Helpers ────────────────────────────────────────────────────────

    private static byte[] BuildExcelBytes(string sheetName, string[] headers, IList<object[]> rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(sheetName);

        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2C3E50");
            cell.Style.Font.FontColor = XLColor.White;
        }

        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var row = rows[rowIndex];
            for (var colIndex = 0; colIndex < row.Length; colIndex++)
                ws.Cell(rowIndex + 2, colIndex + 1).Value = XLCellValue.FromObject(row[colIndex]);
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
