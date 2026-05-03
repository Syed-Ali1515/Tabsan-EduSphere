using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Attendance;
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
            .OrderBy(r => r.StudentName).ThenBy(r => r.CourseCode)
            .ToListAsync(ct);
    }

    public async Task<IList<ResultReportRow>> GetSemesterResultDataAsync(
        Guid semesterId,
        Guid? departmentId,
        CancellationToken ct = default)
    {
        var query = BuildResultQuery(semesterId, null, null, departmentId);
        return await query.OrderBy(r => r.StudentName).ThenBy(r => r.CourseCode).ToListAsync(ct);
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
}
