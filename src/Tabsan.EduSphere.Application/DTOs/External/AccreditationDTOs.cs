// Final-Touches Phase 22 Stage 22.2 — Accreditation Reporting DTOs
namespace Tabsan.EduSphere.Application.DTOs.External;

/// <summary>Summary of an accreditation report template.</summary>
public sealed record AccreditationTemplateDto(
    Guid Id,
    string Name,
    string? Description,
    string Format,
    string? FieldMappingsJson,
    bool IsActive,
    DateTime CreatedAt);

/// <summary>Request to create a new accreditation template.</summary>
public sealed record CreateAccreditationTemplateCommand(
    string Name,
    string? Description,
    string Format,
    string? FieldMappingsJson);

/// <summary>Request to update an existing accreditation template.</summary>
public sealed record UpdateAccreditationTemplateCommand(
    string Name,
    string? Description,
    string Format,
    string? FieldMappingsJson,
    bool IsActive);

/// <summary>Result of generating an accreditation report.</summary>
public sealed record AccreditationReportResult(
    string TemplateName,
    string Format,
    byte[] Content,
    string ContentType,
    string FileName);
