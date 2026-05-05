using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.Academic;
using Tabsan.EduSphere.Domain.Academic;
using Tabsan.EduSphere.Domain.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Manages academic departments.
/// Admin and SuperAdmin can create and modify departments.
/// All authenticated users can read department data.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentRepository _deptRepo;
    private readonly IAuditService _audit;

    public DepartmentController(IDepartmentRepository deptRepo, IAuditService audit)
    {
        _deptRepo = deptRepo;
        _audit = audit;
    }

    // ── GET /api/v1/department ─────────────────────────────────────────────────

    /// <summary>Returns all active departments ordered by name.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var depts = await _deptRepo.GetAllAsync(ct);
        return Ok(depts.Select(d => new { d.Id, d.Name, d.Code, d.IsActive }));
    }

    // ── GET /api/v1/department/{id} ────────────────────────────────────────────

    /// <summary>Returns a single department by its GUID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var dept = await _deptRepo.GetByIdAsync(id, ct);
        return dept is null ? NotFound() : Ok(new { dept.Id, dept.Name, dept.Code, dept.IsActive });
    }

    // ── POST /api/v1/department ────────────────────────────────────────────────

    /// <summary>Creates a new department. Admin and SuperAdmin only.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateDepartmentRequest request, CancellationToken ct)
    {
        if (await _deptRepo.CodeExistsAsync(request.Code, ct))
            return Conflict($"Department code '{request.Code}' is already in use.");

        var dept = new Department(request.Name, request.Code);
        await _deptRepo.AddAsync(dept, ct);
        await _deptRepo.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = dept.Id }, new { dept.Id });
    }

    // ── PUT /api/v1/department/{id} ────────────────────────────────────────────

    /// <summary>Updates the department name. Admin and SuperAdmin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken ct)
    {
        var dept = await _deptRepo.GetByIdAsync(id, ct);
        if (dept is null) return NotFound();

        dept.Rename(request.NewName);
        _deptRepo.Update(dept);
        await _deptRepo.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── DELETE /api/v1/department/{id} (soft-delete) ──────────────────────────

    /// <summary>Soft-deletes (deactivates) a department. Admin and SuperAdmin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var dept = await _deptRepo.GetByIdAsync(id, ct);
        if (dept is null) return NotFound();

        dept.Deactivate();
        _deptRepo.Update(dept);
        await _deptRepo.SaveChangesAsync(ct);
        return NoContent();
    }

    // ── Helper ─────────────────────────────────────────────────────────────────
    private Guid GetUserId()
    {
        var raw = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(raw, out var id) ? id : Guid.Empty;
    }
}

// ── Inline request records (simple enough to keep co-located) ─────────────────

/// <summary>Request body for creating a department.</summary>
public sealed record CreateDepartmentRequest(string Name, string Code);

/// <summary>Request body for updating a department's display name.</summary>
public sealed record UpdateDepartmentRequest(string NewName);
