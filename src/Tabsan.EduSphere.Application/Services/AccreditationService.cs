// Final-Touches Phase 22 Stage 22.2 — AccreditationService: accreditation report generation
using System.Text;
using Tabsan.EduSphere.Application.DTOs.External;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Auditing;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Settings;

namespace Tabsan.EduSphere.Application.Services;

/// <summary>
/// Manages accreditation/government report templates and generates institutional reports.
/// Template CRUD is SuperAdmin only. Report generation is Admin or SuperAdmin.
/// All generate events are written to the audit log.
/// </summary>
public class AccreditationService : IAccreditationService
{
    private readonly IAccreditationRepository _repo;
    private readonly ISettingsRepository      _settings;
    private readonly IAuditService            _audit;

    public AccreditationService(
        IAccreditationRepository repo,
        ISettingsRepository      settings,
        IAuditService            audit)
    {
        _repo     = repo;
        _settings = settings;
        _audit    = audit;
    }

    // ── CRUD ──────────────────────────────────────────────────────────────────

    public async Task<IList<AccreditationTemplateDto>> GetTemplatesAsync(CancellationToken ct = default)
    {
        var all = await _repo.GetAllAsync(ct);
        return all.Select(Map).ToList();
    }

    public async Task<AccreditationTemplateDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var t = await _repo.GetByIdAsync(id, ct)
                ?? throw new KeyNotFoundException($"Accreditation template {id} not found.");
        return Map(t);
    }

    public async Task<AccreditationTemplateDto> CreateAsync(CreateAccreditationTemplateCommand cmd, CancellationToken ct = default)
    {
        var template = new AccreditationTemplate(cmd.Name, cmd.Description, cmd.Format, cmd.FieldMappingsJson);
        await _repo.AddAsync(template, ct);
        await _repo.SaveChangesAsync(ct);
        return Map(template);
    }

    public async Task<AccreditationTemplateDto> UpdateAsync(Guid id, UpdateAccreditationTemplateCommand cmd, CancellationToken ct = default)
    {
        var template = await _repo.GetByIdAsync(id, ct)
                       ?? throw new KeyNotFoundException($"Accreditation template {id} not found.");

        template.Update(cmd.Name, cmd.Description, cmd.Format, cmd.FieldMappingsJson, cmd.IsActive);
        await _repo.SaveChangesAsync(ct);
        return Map(template);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var template = await _repo.GetByIdAsync(id, ct)
                       ?? throw new KeyNotFoundException($"Accreditation template {id} not found.");
        _repo.Remove(template);
        await _repo.SaveChangesAsync(ct);
    }

    // ── Report Generation ─────────────────────────────────────────────────────

    public async Task<AccreditationReportResult> GenerateAsync(Guid id, Guid requestingUserId, CancellationToken ct = default)
    {
        var template = await _repo.GetByIdAsync(id, ct)
                       ?? throw new KeyNotFoundException($"Accreditation template {id} not found.");

        // Fetch institution name for report header
        var settings    = await _settings.GetAllPortalSettingsAsync();
        var institution = settings.TryGetValue("university_name", out var uniName) ? uniName : "Institution";

        // Determine which sections to include (default: all if no mapping defined)
        var sections = ParseSections(template.FieldMappingsJson);

        byte[] content;
        string contentType;
        string fileName;

        if (template.Format == "PDF")
        {
            // Plain-text PDF-like output (actual PDF generation would require a library)
            var sb = new StringBuilder();
            sb.AppendLine($"{institution}");
            sb.AppendLine($"Accreditation Report — {template.Name}");
            sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine(new string('-', 60));
            AppendSections(sb, sections, "\n");
            content     = Encoding.UTF8.GetBytes(sb.ToString());
            contentType = "text/plain";
            fileName    = $"accreditation_{Slug(template.Name)}_{DateTime.UtcNow:yyyyMMdd}.txt";
        }
        else
        {
            // CSV output
            var sb = new StringBuilder();
            sb.AppendLine($"Institution,{CsvEscape(institution)}");
            sb.AppendLine($"Report,{CsvEscape(template.Name)}");
            sb.AppendLine($"Generated,{DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine();
            sb.AppendLine("Section,Key,Value");
            AppendSectionsCsv(sb, sections);
            content     = Encoding.UTF8.GetBytes(sb.ToString());
            contentType = "text/csv";
            fileName    = $"accreditation_{Slug(template.Name)}_{DateTime.UtcNow:yyyyMMdd}.csv";
        }

        // Audit log the export event
        await _audit.LogAsync(new AuditLog(
            "GenerateAccreditationReport",
            "AccreditationTemplate",
            template.Id.ToString(),
            actorUserId: requestingUserId,
            newValuesJson: $"Template: {template.Name} | Format: {template.Format}"), ct);

        return new AccreditationReportResult(template.Name, template.Format, content, contentType, fileName);
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    private static List<string> ParseSections(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<string> { "enrollment", "results", "faculty", "students" };

        try
        {
            var parsed = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
            return parsed?.Count > 0 ? parsed : new List<string> { "enrollment", "results", "faculty", "students" };
        }
        catch
        {
            return new List<string> { "enrollment", "results", "faculty", "students" };
        }
    }

    private static void AppendSections(StringBuilder sb, List<string> sections, string newLine)
    {
        foreach (var section in sections)
        {
            sb.AppendLine($"[{section.ToUpperInvariant()}]");
            sb.AppendLine(SectionDescription(section));
            sb.AppendLine();
        }
    }

    private static void AppendSectionsCsv(StringBuilder sb, List<string> sections)
    {
        foreach (var section in sections)
            sb.AppendLine($"{CsvEscape(section)},description,{CsvEscape(SectionDescription(section))}");
    }

    private static string SectionDescription(string section) => section.ToLower() switch
    {
        "enrollment"  => "Enrollment figures by department and program",
        "results"     => "Pass/fail rates by course and semester",
        "faculty"     => "Active faculty count by department",
        "students"    => "Active student count by department and program",
        "attendance"  => "Attendance rates by course offering",
        "graduation"  => "Graduation applications and approval rates",
        _             => "Custom section"
    };

    private static string Slug(string name)
        => System.Text.RegularExpressions.Regex.Replace(name.ToLowerInvariant(), @"[^a-z0-9]+", "_").Trim('_');

    private static string CsvEscape(string value)
        => value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;

    private static AccreditationTemplateDto Map(AccreditationTemplate t) =>
        new(t.Id, t.Name, t.Description, t.Format, t.FieldMappingsJson, t.IsActive, t.CreatedAt);
}
