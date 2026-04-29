using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages academic semesters.
/// Admin+ can create and close semesters. All authenticated users can read.
/// Closing a semester is a one-way operation — once closed, it cannot be reopened.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class SemesterController : ControllerBase
{
    private readonly ISemesterRepository _repo;

    public SemesterController(ISemesterRepository repo) => _repo = repo;

    // ── GET /api/v1/semester ───────────────────────────────────────────────────

    /// <summary>Returns all semesters ordered by start date descending.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var semesters = await _repo.GetAllAsync(ct);
        return Ok(semesters.Select(s => new
        {
            s.Id, s.Name, s.StartDate, s.EndDate, s.IsClosed, s.ClosedAt
        }));
    }

    // ── GET /api/v1/semester/current ───────────────────────────────────────────

    /// <summary>Returns the most recent open semester, or 404 when all are closed.</summary>
    [HttpGet("current")]
    public async Task<IActionResult> GetCurrent(CancellationToken ct)
    {
        var sem = await _repo.GetCurrentOpenAsync(ct);
        return sem is null ? NotFound("No open semester found.")
            : Ok(new { sem.Id, sem.Name, sem.StartDate, sem.EndDate });
    }

    // ── GET /api/v1/semester/{id} ──────────────────────────────────────────────

    /// <summary>Returns a single semester by its GUID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var sem = await _repo.GetByIdAsync(id, ct);
        return sem is null ? NotFound()
            : Ok(new { sem.Id, sem.Name, sem.StartDate, sem.EndDate, sem.IsClosed, sem.ClosedAt });
    }

    // ── POST /api/v1/semester ──────────────────────────────────────────────────

    /// <summary>Creates a new semester. Admin and SuperAdmin only.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateSemesterRequest request, CancellationToken ct)
    {
        if (request.EndDate <= request.StartDate)
            return BadRequest("End date must be after start date.");

        var sem = new Semester(request.Name, request.StartDate, request.EndDate);
        await _repo.AddAsync(sem, ct);
        await _repo.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = sem.Id }, new { sem.Id });
    }

    // ── POST /api/v1/semester/{id}/close ──────────────────────────────────────

    /// <summary>
    /// Closes the semester permanently. Admin and SuperAdmin only.
    /// Closing is one-way — this operation cannot be reversed.
    /// </summary>
    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
        var sem = await _repo.GetByIdAsync(id, ct);
        if (sem is null) return NotFound();

        try
        {
            sem.Close();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }

        _repo.Update(sem);
        await _repo.SaveChangesAsync(ct);
        return NoContent();
    }
}
