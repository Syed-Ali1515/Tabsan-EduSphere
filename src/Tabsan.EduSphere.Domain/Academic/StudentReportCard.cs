using Tabsan.EduSphere.Domain.Common;
using Tabsan.EduSphere.Domain.Enums;

namespace Tabsan.EduSphere.Domain.Academic;

// Phase 26 — School and College Functional Expansion — Stage 26.2

/// <summary>
/// Stores a generated report-card snapshot for a student in a specific period.
/// The JSON payload is immutable historical output used for download/review.
/// </summary>
public class StudentReportCard : BaseEntity
{
    /// <summary>FK to the student profile.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>Institution mode used for rendering labels/template.</summary>
    public InstitutionType InstitutionType { get; private set; }

    /// <summary>Human-readable academic period label (e.g. Semester 3, Grade 9, Year 2).</summary>
    public string PeriodLabel { get; private set; } = default!;

    /// <summary>Snapshot of report card details (subjects, scores, remarks, summary).</summary>
    public string PayloadJson { get; private set; } = default!;

    /// <summary>User ID that generated this report card.</summary>
    public Guid GeneratedByUserId { get; private set; }

    /// <summary>UTC timestamp of report card generation.</summary>
    public DateTime GeneratedAt { get; private set; }

    private StudentReportCard() { }

    public StudentReportCard(
        Guid studentProfileId,
        InstitutionType institutionType,
        string periodLabel,
        string payloadJson,
        Guid generatedByUserId)
    {
        if (string.IsNullOrWhiteSpace(periodLabel))
            throw new ArgumentException("Period label is required.", nameof(periodLabel));
        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException("Report card payload JSON is required.", nameof(payloadJson));

        StudentProfileId = studentProfileId;
        InstitutionType = institutionType;
        PeriodLabel = periodLabel.Trim();
        PayloadJson = payloadJson;
        GeneratedByUserId = generatedByUserId;
        GeneratedAt = DateTime.UtcNow;
    }
}
