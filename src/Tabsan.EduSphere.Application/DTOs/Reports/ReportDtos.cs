namespace Tabsan.EduSphere.Application.DTOs.Reports;

// ── Report Catalog ─────────────────────────────────────────────────────────────

public record ReportCatalogItemResponse(
    Guid Id,
    string Key,
    string Name,
    string Purpose,
    bool IsActive,
    IReadOnlyList<string> AllowedRoles);

public record ReportCatalogResponse(IReadOnlyList<ReportCatalogItemResponse> Reports);

// ── Attendance Summary ─────────────────────────────────────────────────────────

public record AttendanceSummaryRequest(
    Guid? SemesterId,
    Guid? DepartmentId,
    Guid? CourseOfferingId,
    Guid? StudentProfileId);

public record AttendanceSummaryRow(
    Guid StudentProfileId,
    string RegistrationNumber,
    string StudentName,
    Guid CourseOfferingId,
    string CourseCode,
    string CourseTitle,
    int TotalSessions,
    int AttendedSessions,
    decimal AttendancePercentage);

public record AttendanceSummaryReportResponse(
    IReadOnlyList<AttendanceSummaryRow> Rows,
    int TotalStudents,
    DateTime GeneratedAt);

// ── Result Summary ─────────────────────────────────────────────────────────────

public record ResultSummaryRequest(
    Guid? SemesterId,
    Guid? DepartmentId,
    Guid? CourseOfferingId,
    Guid? StudentProfileId);

public record ResultSummaryRow(
    Guid StudentProfileId,
    string RegistrationNumber,
    string StudentName,
    string CourseCode,
    string CourseTitle,
    string ResultType,
    decimal MarksObtained,
    decimal MaxMarks,
    decimal Percentage,
    DateTime? PublishedAt);

public record ResultSummaryReportResponse(
    IReadOnlyList<ResultSummaryRow> Rows,
    int TotalRecords,
    DateTime GeneratedAt);

// ── GPA Report ─────────────────────────────────────────────────────────────────

public record GpaReportRequest(Guid? DepartmentId, Guid? ProgramId);

public record GpaReportRow(
    Guid StudentProfileId,
    string RegistrationNumber,
    string StudentName,
    string ProgramName,
    string DepartmentName,
    int CurrentSemester,
    decimal Cgpa,
    decimal CurrentSemesterGpa);

public record GpaReportResponse(
    IReadOnlyList<GpaReportRow> Rows,
    decimal AverageCgpa,
    int TotalStudents,
    DateTime GeneratedAt);

// ── Enrollment Summary ─────────────────────────────────────────────────────────

public record EnrollmentSummaryRequest(Guid? SemesterId, Guid? DepartmentId);

public record EnrollmentSummaryRow(
    Guid CourseOfferingId,
    string CourseCode,
    string CourseTitle,
    string SemesterName,
    int MaxEnrollment,
    int EnrolledCount,
    int AvailableSeats);

public record EnrollmentSummaryReportResponse(
    IReadOnlyList<EnrollmentSummaryRow> Rows,
    int TotalOfferings,
    DateTime GeneratedAt);

// ── Semester Results ───────────────────────────────────────────────────────────

public record SemesterResultsRequest(Guid SemesterId, Guid? DepartmentId);

public record SemesterResultsRow(
    Guid StudentProfileId,
    string RegistrationNumber,
    string StudentName,
    string CourseCode,
    string CourseTitle,
    string ResultType,
    decimal MarksObtained,
    decimal MaxMarks,
    decimal Percentage);

public record SemesterResultsReportResponse(
    IReadOnlyList<SemesterResultsRow> Rows,
    int TotalStudents,
    DateTime GeneratedAt);
