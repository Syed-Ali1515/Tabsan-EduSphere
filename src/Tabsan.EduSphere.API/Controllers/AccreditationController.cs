// Final-Touches Phase 22 Stage 22.2 — AccreditationController: accreditation report API
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tabsan.EduSphere.Application.DTOs.External;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Controllers;

/// <summary>
/// Accreditation / government reporting template management and report generation.
/// Template CRUD is SuperAdmin-only; report generation is Admin or SuperAdmin.
/// </summary>
[ApiController]
[Route("api/v1/accreditation")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class AccreditationController : ControllerBase
{
    private readonly IAccreditationService _service;

    public AccreditationController(IAccreditationService service) => _service = service;

    // ── Template CRUD ─────────────────────────────────────────────────────────

    /// <summary>Returns all accreditation templates.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var templates = await _service.GetTemplatesAsync(ct);
        return Ok(templates);
    }

    /// <summary>Returns a single accreditation template by ID.</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var template = await _service.GetByIdAsync(id, ct);
            return Ok(template);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Creates a new accreditation template. SuperAdmin only.</summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Create([FromBody] CreateAccreditationTemplateCommand cmd, CancellationToken ct)
    {
        var created = await _service.CreateAsync(cmd, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing accreditation template. SuperAdmin only.</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccreditationTemplateCommand cmd, CancellationToken ct)
    {
        try
        {
            var updated = await _service.UpdateAsync(id, cmd, ct);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>Deletes an accreditation template. SuperAdmin only.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // ── Report Generation ─────────────────────────────────────────────────────

    /// <summary>
    /// Generates and returns an accreditation report for the given template.
    /// The file is streamed as a download (CSV or plain-text PDF-like).
    /// </summary>
    [HttpGet("{id:guid}/generate")]
    public async Task<IActionResult> Generate(Guid id, CancellationToken ct)
    {
        Guid actorUserId;
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out actorUserId))
            return Forbid();

        try
        {
            var result = await _service.GenerateAsync(id, actorUserId, ct);
            return File(result.Content, result.ContentType, result.FileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
