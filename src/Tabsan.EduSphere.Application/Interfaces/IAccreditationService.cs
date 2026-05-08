// Final-Touches Phase 22 Stage 22.2 — Accreditation Service interface
using Tabsan.EduSphere.Application.DTOs.External;

namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Service for managing accreditation / government report templates and generating reports.
/// All generate events are written to the audit log with user, timestamp, and template name.
/// </summary>
public interface IAccreditationService
{
    /// <summary>Returns all accreditation templates.</summary>
    Task<IList<AccreditationTemplateDto>> GetTemplatesAsync(CancellationToken ct = default);

    /// <summary>Returns a single template by ID.</summary>
    Task<AccreditationTemplateDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Creates a new template. SuperAdmin only.</summary>
    Task<AccreditationTemplateDto> CreateAsync(CreateAccreditationTemplateCommand cmd, CancellationToken ct = default);

    /// <summary>Updates an existing template. SuperAdmin only.</summary>
    Task<AccreditationTemplateDto> UpdateAsync(Guid id, UpdateAccreditationTemplateCommand cmd, CancellationToken ct = default);

    /// <summary>Deletes a template by ID. SuperAdmin only.</summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Generates a report using current institutional data.
    /// Writes an audit log entry for the export event.
    /// </summary>
    Task<AccreditationReportResult> GenerateAsync(Guid id, Guid requestingUserId, CancellationToken ct = default);
}
