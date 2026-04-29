using Tabsan.EduSphere.Domain.Common;

namespace Tabsan.EduSphere.Domain.Assignments;

/// <summary>
/// A transcript export request log record.
/// Created whenever a student's official transcript is generated or downloaded.
/// Rows are append-only; they are never updated or deleted.
/// </summary>
public class TranscriptExportLog : BaseEntity
{
    /// <summary>FK to the student profile whose transcript was exported.</summary>
    public Guid StudentProfileId { get; private set; }

    /// <summary>FK to the user (Admin, student self-service, or system) who requested the export.</summary>
    public Guid RequestedByUserId { get; private set; }

    /// <summary>UTC timestamp of the export request.</summary>
    public DateTime ExportedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional URL or storage path of the generated PDF/document.
    /// Null for on-demand streaming exports where no file is persisted.
    /// </summary>
    public string? DocumentUrl { get; private set; }

    /// <summary>Format of the export (e.g. "PDF", "CSV").</summary>
    public string Format { get; private set; } = default!;

    /// <summary>Client IP address of the requester, captured for audit purposes.</summary>
    public string? IpAddress { get; private set; }

    private TranscriptExportLog() { }

    /// <summary>
    /// Creates a transcript export log entry.
    /// </summary>
    public TranscriptExportLog(Guid studentProfileId, Guid requestedByUserId, string format, string? documentUrl = null, string? ipAddress = null)
    {
        StudentProfileId = studentProfileId;
        RequestedByUserId = requestedByUserId;
        Format = format.ToUpperInvariant();
        DocumentUrl = documentUrl?.Trim();
        IpAddress = ipAddress;
    }
}
