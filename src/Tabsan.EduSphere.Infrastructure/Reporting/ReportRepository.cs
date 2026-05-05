using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Attendance;
using Tabsan.EduSphere.Domain.Fyp;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Settings;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Reporting;

/// <summary>
/// EF Core implementation of <see cref="IReportRepository"/>.
/// All queries use explicit joins for clarity and predictable N+1 avoidance.
/// </summary>
public sealed class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _db;

    public ReportRepository(ApplicationDbContext db) => _db = db;

    // ── Report Catalog ─────────────────────────────────────────────────────────

    public async Task<IList<ReportDefinition>> GetCatalogForRoleAsync(
        string roleName, CancellationToken ct = default)
    {
        // Final-Touches Phase 1 Stage 1.3 — SuperAdmin must always see all active reports.
        if (roleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase))
        {
            return await _db.ReportDefinitions
                .Include(r => r.RoleAssignments)
                .Where(r => r.IsActive)
                .OrderBy(r => r.Name)
                .ToListAsync(ct);
        }

        return await _db.ReportDefinitions
            .Include(r => r.RoleAssignments)
            .Where(r => r.IsActive && r.RoleAssignments.Any(ra => ra.RoleName == roleName))
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    // ── Attendance Data ────────────────────────────────────────────────────────

    public async Task<IList<AttendanceReportRow>> GetAttendanceDataAsync(
        Guid? semesterId,
        Guid? courseOfferingId,
        Guid? studentProfileId,
        CancellationToken ct = default)
    {
        var query =
            from ar  in _db.AttendanceRecords
            join sp  in _db.StudentProfiles  on ar.StudentProfileId  equals sp.Id
            join u   in _db.Users            on sp.UserId            equals u.Id
            join co  in _db.CourseOfferings  on ar.CourseOfferingId  equals co.Id
            join c   in _db.Courses          on co.CourseId          equals c.Id
            join sem in _db.Semesters        on co.SemesterId        equals sem.Id
            join dep in _db.Departments      on c.DepartmentId       equals dep.Id
            where (semesterId        == null || co.SemesterId       == semesterId)
               && (courseOfferingId  == null || ar.CourseOfferingId == courseOfferingId)
               && (studentProfileId  == null || ar.StudentProfileId == studentProfileId)
            select new
            {
                ar.StudentProfileId,
                sp.RegistrationNumber,
                StudentName = u.Username,
                ar.CourseOfferingId,
                CourseCode  = c.Code,
                CourseTitle = c.Title,
                SemesterName    = sem.Name,
                DepartmentName  = dep.Name,
                IsPresent       = ar.Status == AttendanceStatus.Present
            };

        var raw = await query.ToListAsync(ct);

        return raw
            .GroupBy(r => new { r.StudentProfileId, r.RegistrationNumber, r.StudentName, r.CourseOfferingId, r.CourseCode, r.CourseTitle, r.SemesterName, r.DepartmentName })
            .Select(g => new AttendanceReportRow(
                g.Key.StudentProfileId,
                g.Key.RegistrationNumber,
                g.Key.StudentName,
                g.Key.CourseOfferingId,
                g.Key.CourseCode,
                g.Key.CourseTitle,
                g.Key.SemesterName,
                g.Key.DepartmentName,
                TotalSessions:     g.Count(),
                AttendedSessions:  g.Count(r => r.IsPresent),
                AttendancePercentage: g.Count() == 0 ? 0m
                    : Math.Round((decimal)g.Count(r => r.IsPresent) / g.Count() * 100, 2)))
            .OrderBy(r => r.StudentName)
            .ThenBy(r => r.CourseCode)
            .ToList();
    }

    // ── Result Data ────────────────────────────────────────────────────────────

    public async Task<IList<ResultReportRow>> GetResultDataAsync(
        Guid? semesterId,
        Guid? courseOfferingId,
        Guid? studentProfileId,
        CancellationToken ct = default)
    {
        return await BuildResultQuery(semesterId, courseOfferingId, studentProfileId, null)
            .ToListAsync(ct);
    }

    public async Task<IList<ResultReportRow>> GetSemesterResultDataAsync(
        Guid semesterId,
        Guid? departmentId,
        CancellationToken ct = default)
    {
        var query = BuildResultQuery(semesterId, null, null, departmentId);
        return await query.ToListAsync(ct);
    }

    private IQueryable<ResultReportRow> BuildResultQuery(
        Guid? semesterId,
        Guid? courseOfferingId,
        Guid? studentProfileId,
        Guid? departmentId)
    {
        return
            from r   in _db.Results
            join sp  in _db.StudentProfiles  on r.StudentProfileId  equals sp.Id
            join u   in _db.Users            on sp.UserId           equals u.Id
            join co  in _db.CourseOfferings  on r.CourseOfferingId  equals co.Id
            join c   in _db.Courses          on co.CourseId         equals c.Id
            join dep in _db.Departments      on c.DepartmentId      equals dep.Id
            where r.IsPublished
               && (semesterId       == null || co.SemesterId       == semesterId)
               && (courseOfferingId == null || r.CourseOfferingId  == courseOfferingId)
               && (studentProfileId == null || r.StudentProfileId  == studentProfileId)
               && (departmentId     == null || c.DepartmentId      == departmentId)
            orderby u.Username, c.Code
            select new ResultReportRow(
                r.StudentProfileId,
                sp.RegistrationNumber,
                u.Username,
                c.Code,
                c.Title,
                r.ResultType,
                r.MarksObtained,
                r.MaxMarks,
                r.MaxMarks == 0 ? 0m : Math.Round((decimal)r.MarksObtained / r.MaxMarks * 100, 2),
                r.PublishedAt,
                co.SemesterId,
                dep.Name);
    }

    // ── GPA Data ───────────────────────────────────────────────────────────────

    public async Task<IList<GpaReportRow>> GetGpaDataAsync(
        Guid? departmentId,
        Guid? programId,
        CancellationToken ct = default)
    {
        return await (
            from sp  in _db.StudentProfiles
            join u   in _db.Users           on sp.UserId    equals u.Id
            join ap  in _db.AcademicPrograms on sp.ProgramId equals ap.Id
            join dep in _db.Departments      on sp.DepartmentId equals dep.Id
            where (departmentId == null || sp.DepartmentId == departmentId)
               && (programId    == null || sp.ProgramId    == programId)
            orderby u.Username
            select new GpaReportRow(
                sp.Id,
                sp.RegistrationNumber,
                u.Username,
                ap.Name,
                dep.Name,
                sp.CurrentSemesterNumber,
                sp.Cgpa,
                sp.CurrentSemesterGpa)
        ).ToListAsync(ct);
    }

    // ── Enrollment Data ────────────────────────────────────────────────────────

    public async Task<IList<EnrollmentReportRow>> GetEnrollmentDataAsync(
        Guid? semesterId,
        Guid? departmentId,
        CancellationToken ct = default)
    {
        return await (
            from co  in _db.CourseOfferings
            join c   in _db.Courses      on co.CourseId  equals c.Id
            join sem in _db.Semesters    on co.SemesterId equals sem.Id
            join dep in _db.Departments  on c.DepartmentId equals dep.Id
            where (semesterId   == null || co.SemesterId   == semesterId)
               && (departmentId == null || c.DepartmentId  == departmentId)
            orderby c.Code
            select new EnrollmentReportRow(
                co.Id,
                c.Code,
                c.Title,
                sem.Name,
                dep.Name,
                co.MaxEnrollment,
                _db.Enrollments.Count(e => e.CourseOfferingId == co.Id && e.Status == EnrollmentStatus.Active))
        ).ToListAsync(ct);
    }

    // ── Transcript ─────────────────────────────────────────────────────────────

    public async Task<TranscriptReportRepoResult?> GetTranscriptDataAsync(
        Guid studentProfileId,
        CancellationToken ct = default)
    {
        var profile = await (
            from sp  in _db.StudentProfiles
            join u   in _db.Users           on sp.UserId      equals u.Id
            join ap  in _db.AcademicPrograms on sp.ProgramId   equals ap.Id
            join dep in _db.Departments      on sp.DepartmentId equals dep.Id
            where sp.Id == studentProfileId
            select new
            {
                sp.Id,
                sp.RegistrationNumber,
                sp.Cgpa,
                StudentName  = u.Username,
                ProgramName  = ap.Name,
                DepartmentName = dep.Name
            }
        ).FirstOrDefaultAsync(ct);

        if (profile is null) return null;

        var rows = await (
            from r   in _db.Results
            join co  in _db.CourseOfferings on r.CourseOfferingId equals co.Id
            join c   in _db.Courses         on co.CourseId        equals c.Id
            join sem in _db.Semesters       on co.SemesterId      equals sem.Id
            where r.StudentProfileId == studentProfileId && r.IsPublished
            orderby sem.Name, c.Code
            select new TranscriptResultRow(
                c.Code,
                c.Title,
                sem.Name,
                r.ResultType,
                r.MarksObtained,
                r.MaxMarks,
                r.MaxMarks == 0 ? 0m : Math.Round((decimal)r.MarksObtained / r.MaxMarks * 100, 2),
                r.GradePoint,
                r.PublishedAt)
        ).ToListAsync(ct);

        return new TranscriptReportRepoResult(
            profile.Id,
            profile.RegistrationNumber,
            profile.StudentName,
            profile.ProgramName,
            profile.DepartmentName,
            profile.Cgpa,
            rows);
    }

    // ── Low Attendance Warning ─────────────────────────────────────────────────

    public async Task<IList<LowAttendanceReportRow>> GetLowAttendanceDataAsync(
        decimal thresholdPercent,
        Guid? departmentId,
        Guid? courseOfferingId,
        CancellationToken ct = default)
    {
        var query =
            from ar  in _db.AttendanceRecords
            join sp  in _db.StudentProfiles  on ar.StudentProfileId  equals sp.Id
            join u   in _db.Users            on sp.UserId            equals u.Id
            join co  in _db.CourseOfferings  on ar.CourseOfferingId  equals co.Id
            join c   in _db.Courses          on co.CourseId          equals c.Id
            join sem in _db.Semesters        on co.SemesterId        equals sem.Id
            join dep in _db.Departments      on c.DepartmentId       equals dep.Id
            where (courseOfferingId == null || ar.CourseOfferingId == courseOfferingId)
               && (departmentId     == null || c.DepartmentId      == departmentId)
            select new
            {
                ar.StudentProfileId,
                sp.RegistrationNumber,
                StudentName    = u.Username,
                CourseCode     = c.Code,
                CourseTitle    = c.Title,
                SemesterName   = sem.Name,
                DepartmentName = dep.Name,
                IsPresent      = ar.Status == AttendanceStatus.Present
            };

        var raw = await query.ToListAsync(ct);

        return raw
            .GroupBy(r => new { r.StudentProfileId, r.RegistrationNumber, r.StudentName,
                                r.CourseCode, r.CourseTitle, r.SemesterName, r.DepartmentName })
            .Select(g =>
            {
                var total    = g.Count();
                var attended = g.Count(x => x.IsPresent);
                var pct      = total == 0 ? 0m : Math.Round((decimal)attended / total * 100, 2);
                return new LowAttendanceReportRow(
                    g.Key.StudentProfileId,
                    g.Key.RegistrationNumber,
                    g.Key.StudentName,
                    g.Key.CourseCode,
                    g.Key.CourseTitle,
                    g.Key.SemesterName,
                    g.Key.DepartmentName,
                    total, attended, pct);
            })
            .Where(r => r.AttendancePercentage < thresholdPercent)
            .OrderBy(r => r.AttendancePercentage)
            .ThenBy(r => r.StudentName)
            .ToList();
    }

    // ── FYP Status ─────────────────────────────────────────────────────────────

    public async Task<IList<FypStatusReportRow>> GetFypStatusDataAsync(
        Guid? departmentId,
        string? status,
        CancellationToken ct = default)
    {
        FypProjectStatus? statusFilter = string.IsNullOrWhiteSpace(status)
            ? null
            : Enum.TryParse<FypProjectStatus>(status, ignoreCase: true, out var parsed)
                ? parsed
                : (FypProjectStatus?)null;

        return await (
            from p   in _db.FypProjects
            join sp  in _db.StudentProfiles  on p.StudentProfileId equals sp.Id
            join u   in _db.Users            on sp.UserId          equals u.Id
            join dep in _db.Departments      on p.DepartmentId     equals dep.Id
            join sup in _db.Users.DefaultIfEmpty() on p.SupervisorUserId equals sup!.Id into supGroup
            from sup in supGroup.DefaultIfEmpty()
            where (departmentId  == null || p.DepartmentId == departmentId)
               && (statusFilter  == null || p.Status       == statusFilter)
            orderby p.CreatedAt descending
            select new FypStatusReportRow(
                p.Id,
                p.Title,
                u.Username,
                sp.RegistrationNumber,
                dep.Name,
                sup == null ? null : sup.Username,
                p.Status.ToString(),
                p.CreatedAt,
                _db.FypMeetings.Count(m => m.FypProjectId == p.Id))
        ).ToListAsync(ct);
    }
}
