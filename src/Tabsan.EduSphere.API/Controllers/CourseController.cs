using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages the course catalogue and course offerings.
/// Admin+ manages courses and offerings; Faculty can read their own offerings; Students can browse.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class CourseController : ControllerBase
{
    private readonly ICourseRepository _repo;
    private readonly IFacultyAssignmentRepository _facultyAssignments;

    public CourseController(ICourseRepository repo, IFacultyAssignmentRepository facultyAssignments)
    {
        _repo = repo;
        _facultyAssignments = facultyAssignments;
    }

    // ────────────────────── COURSES ────────────────────────────────────────────

    // ── GET /api/v1/course ─────────────────────────────────────────────────────

    /// <summary>Returns the course catalogue, optionally filtered by departmentId.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? departmentId, CancellationToken ct)
    {
        var courses = await _repo.GetAllAsync(departmentId, ct);
        return Ok(courses.Select(c => new
        {
            c.Id, c.Title, c.Code, c.CreditHours, c.DepartmentId,
            DepartmentName = c.Department?.Name ?? "", c.IsActive
        }));
    }

    // ── GET /api/v1/course/{id} ────────────────────────────────────────────────

    /// <summary>Returns a single course definition by its GUID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var c = await _repo.GetByIdAsync(id, ct);
        return c is null ? NotFound()
            : Ok(new { c.Id, c.Title, c.Code, c.CreditHours, c.DepartmentId, c.IsActive });
    }

    // ── POST /api/v1/course ────────────────────────────────────────────────────

    /// <summary>Adds a new course to the catalogue. Admin and SuperAdmin only.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCourseRequest request, CancellationToken ct)
    {
        if (await _repo.CodeExistsAsync(request.Code, request.DepartmentId, ct))
            return Conflict($"Course code '{request.Code}' already exists in this department.");

        var course = new Course(request.Title, request.Code, request.CreditHours, request.DepartmentId);
        await _repo.AddAsync(course, ct);
        await _repo.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = course.Id }, new { course.Id });
    }

    // ── PUT /api/v1/course/{id}/title ──────────────────────────────────────────

    /// <summary>Updates the course title. Admin and SuperAdmin only.</summary>
    [HttpPut("{id:guid}/title")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateTitle(Guid id, [FromBody] UpdateCourseTitleRequest request, CancellationToken ct)
    {
        var course = await _repo.GetByIdAsync(id, ct);
        if (course is null) return NotFound();

        course.UpdateTitle(request.NewTitle);
        _repo.Update(course);
        await _repo.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── DELETE /api/v1/course/{id} ─────────────────────────────────────────────

    /// <summary>Soft-deactivates a course. SuperAdmin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var course = await _repo.GetByIdAsync(id, ct);
        if (course is null) return NotFound();

        course.Deactivate();
        _repo.Update(course);
        await _repo.SaveChangesAsync(ct);
        return NoContent();
    }

    // ────────────────────── OFFERINGS ──────────────────────────────────────────

    // ── GET /api/v1/course/offerings?semesterId={id} or ?departmentId={id} ──────

    /// <summary>Returns all offerings for the given semester or department.</summary>
    [HttpGet("offerings")]
    public async Task<IActionResult> GetOfferings([FromQuery] Guid? semesterId, [FromQuery] Guid? departmentId, CancellationToken ct)
    {
        IReadOnlyList<CourseOffering> offerings;
        
        if (departmentId.HasValue)
            offerings = await _repo.GetOfferingsByDepartmentAsync(departmentId.Value, ct);
        else if (semesterId.HasValue)
            offerings = await _repo.GetOfferingsBySemesterAsync(semesterId.Value, ct);
        else
            offerings = new List<CourseOffering>();

        return Ok(offerings.Select(o => new
        {
            o.Id, o.CourseId, CourseCode = o.Course.Code, CourseName = o.Course.Title, 
            o.SemesterId, SemesterName = o.Semester.Name, o.FacultyUserId,
            o.MaxEnrollment, o.IsOpen
        }));
    }

    // ── GET /api/v1/course/offerings/my ───────────────────────────────────────

    /// <summary>
    /// Returns all offerings assigned to the calling faculty member.
    /// Data is further constrained to departments the faculty is assigned to.
    /// </summary>
    [HttpGet("offerings/my")]
    [Authorize(Roles = "Faculty")]
    public async Task<IActionResult> GetMyOfferings(CancellationToken ct)
    {
        var facultyId = GetUserId();
        if (facultyId == Guid.Empty) return Forbid();

        var allowedDepts = await _facultyAssignments.GetDepartmentIdsForFacultyAsync(facultyId, ct);
        var offerings = await _repo.GetOfferingsByFacultyAsync(facultyId, ct);

        // Filter to only offerings whose course belongs to an assigned department.
        var filtered = offerings.Where(o => allowedDepts.Contains(o.Course.DepartmentId));
        return Ok(filtered.Select(o => new
        {
            o.Id, CourseTitle = o.Course.Title, SemesterName = o.Semester.Name,
            o.MaxEnrollment, o.IsOpen
        }));
    }

    // ── POST /api/v1/course/offerings ─────────────────────────────────────────

    /// <summary>Creates a course offering for a semester. Admin and SuperAdmin only.</summary>
    [HttpPost("offerings")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> CreateOffering([FromBody] CreateOfferingRequest request, CancellationToken ct)
    {
        var offering = new CourseOffering(request.CourseId, request.SemesterId, request.MaxEnrollment, request.FacultyUserId);
        await _repo.AddOfferingAsync(offering, ct);
        await _repo.SaveChangesAsync(ct);
        return Created($"/api/v1/course/offerings/{offering.Id}", new { offering.Id });
    }

    // ── PUT /api/v1/course/offerings/{id}/faculty ──────────────────────────────

    /// <summary>Assigns or re-assigns faculty to a course offering. Admin and SuperAdmin only.</summary>
    [HttpPut("offerings/{id:guid}/faculty")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AssignFaculty(Guid id, [FromBody] AssignFacultyRequest request, CancellationToken ct)
    {
        var offering = await _repo.GetOfferingByIdAsync(id, ct);
        if (offering is null) return NotFound();

        offering.AssignFaculty(request.FacultyUserId);
        _repo.UpdateOffering(offering);
        await _repo.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Helper ─────────────────────────────────────────────────────────────────
    private Guid GetUserId()
    {
        var raw = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
