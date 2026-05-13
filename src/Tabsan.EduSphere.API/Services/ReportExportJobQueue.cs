using System.Threading.Channels;

namespace Tabsan.EduSphere.API.Services;

public enum ReportExportFormat
{
    Excel,
    Csv,
    Pdf
}

public sealed record ResultSummaryExportJobRequest(
    Guid JobId,
    Guid RequestedByUserId,
    Guid? SemesterId,
    Guid? DepartmentId,
    Guid? CourseOfferingId,
    Guid? StudentProfileId,
    int? InstitutionType,
    ReportExportFormat Format);

public sealed class ReportExportJobQueue
{
    private readonly Channel<ResultSummaryExportJobRequest> _channel = Channel.CreateUnbounded<ResultSummaryExportJobRequest>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public void Enqueue(ResultSummaryExportJobRequest workItem)
    {
        if (!_channel.Writer.TryWrite(workItem))
            throw new InvalidOperationException("Unable to queue report export job.");
    }

    public IAsyncEnumerable<ResultSummaryExportJobRequest> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}

public sealed class ReportExportJobState
{
    public Guid JobId { get; init; }
    public Guid RequestedByUserId { get; init; }
    public string Status { get; init; } = "queued";
    public string? Error { get; init; }
    public string? ContentType { get; init; }
    public string? FileName { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; init; }
}