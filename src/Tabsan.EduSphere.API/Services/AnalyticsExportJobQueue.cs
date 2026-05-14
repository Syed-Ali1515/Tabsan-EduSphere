using System.Threading.Channels;

namespace Tabsan.EduSphere.API.Services;

public enum AnalyticsExportReportType
{
    Performance,
    Attendance
}

public enum AnalyticsExportFormat
{
    Pdf,
    Excel
}

public sealed record AnalyticsExportJobRequest(
    Guid JobId,
    Guid RequestedByUserId,
    Guid? DepartmentId,
    int? InstitutionType,
    AnalyticsExportReportType ReportType,
    AnalyticsExportFormat Format);

public sealed class AnalyticsExportJobQueue
{
    private readonly Channel<AnalyticsExportJobRequest> _channel = Channel.CreateUnbounded<AnalyticsExportJobRequest>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public void Enqueue(AnalyticsExportJobRequest workItem)
    {
        if (!_channel.Writer.TryWrite(workItem))
            throw new InvalidOperationException("Unable to queue analytics export job.");
    }

    public IAsyncEnumerable<AnalyticsExportJobRequest> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}

public sealed class AnalyticsExportJobState
{
    public Guid JobId { get; init; }
    public Guid RequestedByUserId { get; init; }
    public AnalyticsExportReportType ReportType { get; init; }
    public AnalyticsExportFormat Format { get; init; }
    public string Status { get; init; } = "queued";
    public string? Error { get; init; }
    public string? ContentType { get; init; }
    public string? FileName { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; init; }
}
