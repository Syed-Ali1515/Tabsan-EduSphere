using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.Dtos;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages department timetables.
/// Admins/SuperAdmins can create, edit, publish, and delete timetables.
/// All authenticated users can read published timetables and download exports.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TimetableController : ControllerBase
{
    private readonly ITimetableService _service;
    private readonly Tabsan.EduSphere.Domain.Interfaces.IUserRepository _users;

    public TimetableController(
        ITimetableService service,
        Tabsan.EduSphere.Domain.Interfaces.IUserRepository users)
    {
        _service = service;
        _users = users;
    }

    // ── GET /api/v1/timetable/department/{departmentId} ───────────────────────

    /// <summary>
    /// Returns all timetables for a department.
    /// Admins/SuperAdmins see both draft and published; other roles see published only.
    /// </summary>
    [HttpGet("department/{departmentId:guid}")]
    public async Task<IActionResult> GetByDepartment(Guid departmentId, CancellationToken ct)
    {
        bool publishedOnly = !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin");
        var list = await _service.GetByDepartmentAsync(departmentId, publishedOnly, ct);
        return Ok(list);
    }

    // ── GET /api/v1/timetable/{id} ────────────────────────────────────────────

    /// <summary>Returns a full timetable with all entries by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var dto = await _service.GetByIdAsync(id, ct);
            // Non-admin users can only see published timetables
            if (!dto.IsPublished && !User.IsInRole("Admin") && !User.IsInRole("SuperAdmin"))
                return Forbid();

            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/timetable ────────────────────────────────────────────────

    /// <summary>Creates a new draft timetable.</summary>
    [HttpPost]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateTimetableCommand cmd, CancellationToken ct)
    {
        var dto = await _service.CreateAsync(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
    }

    // ── PUT /api/v1/timetable/{id} ────────────────────────────────────────────

    /// <summary>Updates the timetable title.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTimetableCommand cmd, CancellationToken ct)
    {
        try
        {
            var dto = await _service.UpdateAsync(id, cmd, ct);
            return Ok(dto);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/timetable/{id}/entries ───────────────────────────────────

    /// <summary>Adds a new scheduled slot to the timetable.</summary>
    [HttpPost("{id:guid}/entries")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> AddEntry(Guid id, [FromBody] UpsertTimetableEntryCommand cmd, CancellationToken ct)
    {
        try
        {
            var entry = await _service.AddEntryAsync(id, cmd, ct);
            return Ok(entry);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── PUT /api/v1/timetable/{id}/entries/{entryId} ──────────────────────────

    /// <summary>Updates an existing timetable entry.</summary>
    [HttpPut("{id:guid}/entries/{entryId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateEntry(
        Guid id, Guid entryId, [FromBody] UpsertTimetableEntryCommand cmd, CancellationToken ct)
    {
        try
        {
            var entry = await _service.UpdateEntryAsync(id, entryId, cmd, ct);
            return Ok(entry);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── DELETE /api/v1/timetable/{id}/entries/{entryId} ───────────────────────

    /// <summary>Removes a scheduled slot from the timetable.</summary>
    [HttpDelete("{id:guid}/entries/{entryId:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> DeleteEntry(Guid id, Guid entryId, CancellationToken ct)
    {
        try
        {
            await _service.DeleteEntryAsync(id, entryId, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/timetable/{id}/publish ───────────────────────────────────

    /// <summary>Publishes the timetable so it is visible to all department members.</summary>
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.PublishAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── POST /api/v1/timetable/{id}/unpublish ─────────────────────────────────

    /// <summary>Unpublishes the timetable (returns it to draft for editing).</summary>
    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Unpublish(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.UnpublishAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── DELETE /api/v1/timetable/{id} ─────────────────────────────────────────

    /// <summary>Soft-deletes the timetable. Data is preserved in the database.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET /api/v1/timetable/{id}/export/excel ───────────────────────────────

    /// <summary>Downloads the timetable as an Excel (.xlsx) file.</summary>
    [HttpGet("{id:guid}/export/excel")]
    public async Task<IActionResult> ExportExcel(Guid id, CancellationToken ct)
    {
        try
        {
            var bytes = await _service.ExportExcelAsync(id, ct);
            return File(bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"timetable-{id:N}.xlsx");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET /api/v1/timetable/{id}/export/pdf ─────────────────────────────────

    /// <summary>Downloads the timetable as a PDF file.</summary>
    [HttpGet("{id:guid}/export/pdf")]
    public async Task<IActionResult> ExportPdf(Guid id, CancellationToken ct)
    {
        try
        {
            var bytes = await _service.ExportPdfAsync(id, ct);
            return File(bytes, "application/pdf", $"timetable-{id:N}.pdf");
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ── GET /api/v1/timetable/mine/teacher ────────────────────────────────────

    /// <summary>
    /// Returns all published timetable slots assigned to the currently authenticated faculty member.
    /// Accessible to Faculty, Admin, and SuperAdmin roles.
    /// </summary>
    [HttpGet("mine/teacher")]
    [Authorize(Roles = "Faculty,Admin,SuperAdmin")]
    public async Task<IActionResult> GetMyTeacherTimetable(CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { message = "User identity could not be resolved." });

        var entries = await _service.GetForTeacherAsync(userId, ct);
        return Ok(entries);
    }

    // ── GET /api/v1/timetable/faculty ─────────────────────────────────────────

    /// <summary>
    /// Returns active faculty users for timetable teacher dropdown selection.
    /// Admin and SuperAdmin only.
    /// </summary>
    [HttpGet("faculty")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> GetFacultyUsers(CancellationToken ct)
    {
        var users = await _users.GetFacultyUsersAsync(ct);
        return Ok(users.Select(u => new
        {
            u.Id,
            u.Username,
            u.Email,
            u.DepartmentId,
            DisplayName = string.IsNullOrWhiteSpace(u.Email)
                ? u.Username
                : $"{u.Username} ({u.Email})"
        }));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Guid GetUserId()
    {
        var raw = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}
