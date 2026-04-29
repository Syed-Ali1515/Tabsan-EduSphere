using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages degree programmes.
/// Admin+ can perform full CRUD; all authenticated users can read.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ProgramController : ControllerBase
{
    private readonly IAcademicProgramRepository _repo;
    private readonly IDepartmentRepository _deptRepo;

    public ProgramController(IAcademicProgramRepository repo, IDepartmentRepository deptRepo)
    {
        _repo = repo;
        _deptRepo = deptRepo;
    }

    // ── GET /api/v1/program ────────────────────────────────────────────────────

    /// <summary>Returns all programmes, optionally filtered by departmentId query parameter.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? departmentId, CancellationToken ct)
    {
        var programs = await _repo.GetAllAsync(departmentId, ct);
        return Ok(programs.Select(p => new
        {
            p.Id, p.Name, p.Code, p.DepartmentId, p.TotalSemesters, p.IsActive
        }));
    }

    // ── GET /api/v1/program/{id} ───────────────────────────────────────────────

    /// <summary>Returns a single programme by its GUID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var prog = await _repo.GetByIdAsync(id, ct);
        return prog is null ? NotFound()
            : Ok(new { prog.Id, prog.Name, prog.Code, prog.DepartmentId, prog.TotalSemesters, prog.IsActive });
    }

    // ── POST /api/v1/program ───────────────────────────────────────────────────

    /// <summary>Creates a new degree programme. Admin and SuperAdmin only.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProgramRequest request, CancellationToken ct)
    {
        if (await _repo.CodeExistsAsync(request.Code, ct))
            return Conflict($"Programme code '{request.Code}' is already in use.");

        if (await _deptRepo.GetByIdAsync(request.DepartmentId, ct) is null)
            return BadRequest("Department not found.");

        var prog = new AcademicProgram(request.Name, request.Code, request.DepartmentId, request.TotalSemesters);
        await _repo.AddAsync(prog, ct);
        await _repo.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = prog.Id }, new { prog.Id });
    }

    // ── PUT /api/v1/program/{id} ───────────────────────────────────────────────

    /// <summary>Renames an existing programme. Admin and SuperAdmin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProgramRequest request, CancellationToken ct)
    {
        var prog = await _repo.GetByIdAsync(id, ct);
        if (prog is null) return NotFound();

        prog.Rename(request.Name);
        _repo.Update(prog);
        await _repo.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── DELETE /api/v1/program/{id} ────────────────────────────────────────────

    /// <summary>Soft-deactivates a programme. SuperAdmin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var prog = await _repo.GetByIdAsync(id, ct);
        if (prog is null) return NotFound();

        prog.Deactivate();
        _repo.Update(prog);
        await _repo.SaveChangesAsync(ct);
        return NoContent();
    }
}
