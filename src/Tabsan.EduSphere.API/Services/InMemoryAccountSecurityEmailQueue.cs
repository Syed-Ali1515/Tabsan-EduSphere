using System.Threading.Channels;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

// Final-Touches Phase 34 Stage 7.1 — in-memory queue for offloading account-security email sends from request path.
public sealed class InMemoryAccountSecurityEmailQueue : IAccountSecurityEmailQueue
{
    private readonly Channel<AccountSecurityEmailWorkItem> _channel = Channel.CreateUnbounded<AccountSecurityEmailWorkItem>(
        new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public ValueTask EnqueueAsync(AccountSecurityEmailWorkItem workItem, CancellationToken ct = default)
    {
        if (!_channel.Writer.TryWrite(workItem))
            throw new InvalidOperationException("Unable to queue account-security email work item.");

        return ValueTask.CompletedTask;
    }

    public IAsyncEnumerable<AccountSecurityEmailWorkItem> DequeueAllAsync(CancellationToken ct)
        => _channel.Reader.ReadAllAsync(ct);
}
