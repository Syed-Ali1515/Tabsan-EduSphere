using Microsoft.EntityFrameworkCore;
using Tabsan.EduSphere.Application.DTOs.Search;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ISearchRepository for Phase 13 — Global Search.
/// Performs cross-entity LIKE queries directly against the database, avoiding
/// in-memory filtering over potentially large result sets.
/// </summary>
public sealed class SearchRepository : ISearchRepository
{
    private readonly ApplicationDbContext _db;

    public SearchRepository(ApplicationDbContext db) => _db = db;

    // ── Students ─────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SearchResultItem>> SearchStudentsAsync(
        string               term,
        IReadOnlyList<Guid>? departmentIds,
        int                  limit,
        CancellationToken    ct = default)
    {
        var query =
            from sp in _db.StudentProfiles
            join u  in _db.Users on sp.UserId equals u.Id
            where !sp.IsDeleted && !u.IsDeleted
               && (u.Username.Contains(term) || sp.RegistrationNumber.Contains(term))
               && (departmentIds == null || departmentIds.Contains(sp.DepartmentId))
            orderby u.Username
            select new
            {
                sp.Id,
                u.Username,
                sp.RegistrationNumber
            };

        var rows = await query.Take(limit).ToListAsync(ct);

        return rows
            .Select(r => new SearchResultItem(
                "Student",
                r.Id,
                r.Username,
                r.RegistrationNumber,
                "/Portal/Students"))
            .ToList();
    }

    // ── Courses ──────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SearchResultItem>> SearchCoursesAsync(
        string               term,
        IReadOnlyList<Guid>? departmentIds,
        int                  limit,
        CancellationToken    ct = default)
    {
        var query =
            from c in _db.Courses
            where !c.IsDeleted
               && (c.Title.Contains(term) || c.Code.Contains(term))
               && (departmentIds == null || departmentIds.Contains(c.DepartmentId))
            orderby c.Title
            select new { c.Id, c.Title, c.Code };

        var rows = await query.Take(limit).ToListAsync(ct);

        return rows
            .Select(r => new SearchResultItem("Course", r.Id, r.Title, r.Code, "/Portal/Courses"))
            .ToList();
    }

    // ── Course Offerings ──────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SearchResultItem>> SearchOfferingsAsync(
        string               term,
        IReadOnlyList<Guid>? departmentIds,
        Guid?                facultyUserId,
        int                  limit,
        CancellationToken    ct = default)
    {
        var query =
            from co  in _db.CourseOfferings
            join c   in _db.Courses   on co.CourseId   equals c.Id
            join sem in _db.Semesters on co.SemesterId equals sem.Id
            where !co.IsDeleted && !c.IsDeleted
               && (c.Title.Contains(term) || c.Code.Contains(term) || sem.Name.Contains(term))
               && (departmentIds  == null || departmentIds.Contains(c.DepartmentId))
               && (facultyUserId  == null || co.FacultyUserId == facultyUserId)
            orderby c.Title, sem.Name
            select new { co.Id, CourseTitle = c.Title, c.Code, SemesterName = sem.Name };

        var rows = await query.Take(limit).ToListAsync(ct);

        return rows
            .Select(r => new SearchResultItem(
                "CourseOffering",
                r.Id,
                r.CourseTitle,
                $"{r.Code} — {r.SemesterName}",
                "/Portal/Courses"))
            .ToList();
    }

    // ── Faculty ───────────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SearchResultItem>> SearchFacultyAsync(
        string               term,
        IReadOnlyList<Guid>? departmentIds,
        int                  limit,
        CancellationToken    ct = default)
    {
        var query =
            from u in _db.Users
            join r in _db.Roles on u.RoleId equals r.Id
            where !u.IsDeleted && r.Name == "Faculty"
               && (u.Username.Contains(term) || (u.Email != null && u.Email.Contains(term)))
               && (departmentIds == null || (u.DepartmentId != null && departmentIds.Contains(u.DepartmentId.Value)))
            orderby u.Username
            select new { u.Id, u.Username, u.Email };

        var rows = await query.Take(limit).ToListAsync(ct);

        return rows
            .Select(r => new SearchResultItem(
                "Faculty",
                r.Id,
                r.Username,
                r.Email ?? "",
                "/Portal/AdminUsers"))
            .ToList();
    }

    // ── Departments ───────────────────────────────────────────────────────────

    public async Task<IReadOnlyList<SearchResultItem>> SearchDepartmentsAsync(
        string               term,
        IReadOnlyList<Guid>? allowedIds,
        int                  limit,
        CancellationToken    ct = default)
    {
        var query =
            from d in _db.Departments
            where !d.IsDeleted
               && (d.Name.Contains(term) || d.Code.Contains(term))
               && (allowedIds == null || allowedIds.Contains(d.Id))
            orderby d.Name
            select new { d.Id, d.Name, d.Code };

        var rows = await query.Take(limit).ToListAsync(ct);

        return rows
            .Select(r => new SearchResultItem("Department", r.Id, r.Name, r.Code, "/Portal/Departments"))
            .ToList();
    }

    // ── Student helpers ───────────────────────────────────────────────────────

    public Task<Guid?> GetStudentProfileIdByUserIdAsync(Guid userId, CancellationToken ct = default)
        => _db.StudentProfiles
              .Where(sp => sp.UserId == userId && !sp.IsDeleted)
              .Select(sp => (Guid?)sp.Id)
              .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<SearchResultItem>> SearchStudentEnrolledOfferingsAsync(
        Guid              studentProfileId,
        string            term,
        int               limit,
        CancellationToken ct = default)
    {
        var query =
            from e   in _db.Enrollments
            join co  in _db.CourseOfferings on e.CourseOfferingId equals co.Id
            join c   in _db.Courses         on co.CourseId        equals c.Id
            join sem in _db.Semesters       on co.SemesterId      equals sem.Id
            where e.StudentProfileId == studentProfileId
               && !co.IsDeleted && !c.IsDeleted
               && (c.Title.Contains(term) || c.Code.Contains(term) || sem.Name.Contains(term))
            orderby c.Title
            select new { co.Id, CourseTitle = c.Title, c.Code, SemesterName = sem.Name };

        var rows = await query.Take(limit).ToListAsync(ct);

        return rows
            .Select(r => new SearchResultItem(
                "CourseOffering",
                r.Id,
                r.CourseTitle,
                $"{r.Code} — {r.SemesterName}",
                "/Portal/Courses"))
            .ToList();
    }
}
