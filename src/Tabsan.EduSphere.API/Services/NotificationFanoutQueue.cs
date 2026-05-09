using System.Threading.Channels;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

public sealed class NotificationFanoutQueue : INotificationFanoutQueue
{
    private readonly Channel<NotificationFanoutWorkItem> _channel = Channel.CreateUnbounded<NotificationFanoutWorkItem>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public void Enqueue(NotificationFanoutWorkItem workItem)
    {
        if (!_channel.Writer.TryWrite(workItem))
            throw new InvalidOperationException("Unable to queue notification fan-out work item.");
    }

    public IAsyncEnumerable<NotificationFanoutWorkItem> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}