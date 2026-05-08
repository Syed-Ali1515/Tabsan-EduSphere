// Final-Touches Phase 22 Stage 22.2 — AccreditationTemplate domain entity
using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Settings;

/// <summary>
/// Defines a named accreditation / government report template.
/// SuperAdmin creates and manages templates; Admin and SuperAdmin can generate reports.
/// All generate events are written to the audit log.
/// </summary>
public class AccreditationTemplate : BaseEntity
{
    /// <summary>Display name of the template (e.g. "HEC Enrollment Report").</summary>
    public string Name { get; private set; } = default!;

    /// <summary>Optional description of the regulatory purpose.</summary>
    public string? Description { get; private set; }

    /// <summary>Output format — "CSV" or "PDF".</summary>
    public string Format { get; private set; } = "CSV";

    /// <summary>JSON array of section keys to include (e.g. ["enrollment","results","faculty"]).</summary>
    public string? FieldMappingsJson { get; private set; }

    /// <summary>When false, the template is hidden from the generation UI.</summary>
    public bool IsActive { get; private set; } = true;

    // ── Constructors ─────────────────────────────────────────────────────────

    protected AccreditationTemplate() { }   // EF Core

    public AccreditationTemplate(string name, string? description, string format, string? fieldMappingsJson)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required.", nameof(name));

        Name               = name.Trim();
        Description        = description?.Trim();
        Format             = format == "PDF" ? "PDF" : "CSV";
        FieldMappingsJson  = fieldMappingsJson;
        IsActive           = true;
    }

    // ── Mutation methods ──────────────────────────────────────────────────────

    public void Update(string name, string? description, string format, string? fieldMappingsJson, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Template name is required.", nameof(name));

        Name              = name.Trim();
        Description       = description?.Trim();
        Format            = format == "PDF" ? "PDF" : "CSV";
        FieldMappingsJson = fieldMappingsJson;
        IsActive          = isActive;
        Touch();
    }
}
