using System.Threading.Channels;

namespace Tabsan.EduSphere.API.Services;

public sealed record ResultPublishJobWorkItem(Guid JobId, Guid CourseOfferingId, Guid RequestedByUserId);

public sealed class ResultPublishJobQueue
{
    private readonly Channel<ResultPublishJobWorkItem> _channel = Channel.CreateUnbounded<ResultPublishJobWorkItem>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public void Enqueue(ResultPublishJobWorkItem workItem)
    {
        if (!_channel.Writer.TryWrite(workItem))
            throw new InvalidOperationException("Unable to queue result publish job.");
    }

    public IAsyncEnumerable<ResultPublishJobWorkItem> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}

public sealed class ResultPublishJobState
{
    public Guid JobId { get; init; }
    public Guid CourseOfferingId { get; init; }
    public Guid RequestedByUserId { get; init; }
    public string Status { get; init; } = "queued";
    public int? PublishedCount { get; init; }
    public string? Error { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; init; }
}