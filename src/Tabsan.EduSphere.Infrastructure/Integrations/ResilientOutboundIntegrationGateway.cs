using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Integrations;

// Final-Touches Phase 30 Stage 30.1 - unified resilient gateway with retry/timeout/dead-letter handling.
public sealed class ResilientOutboundIntegrationGateway : IOutboundIntegrationGateway
{
    private const string DeadLetterCacheKey = "integration-gateway:dead-letters";
    private static readonly SemaphoreSlim DeadLetterLock = new(1, 1);

    private readonly IDistributedCache _cache;
    private readonly IOptionsMonitor<IntegrationGatewayOptions> _options;
    private readonly ILogger<ResilientOutboundIntegrationGateway> _logger;

    public ResilientOutboundIntegrationGateway(
        IDistributedCache cache,
        IOptionsMonitor<IntegrationGatewayOptions> options,
        ILogger<ResilientOutboundIntegrationGateway> logger)
    {
        _cache = cache;
        _options = options;
        _logger = logger;
    }

    public async Task ExecuteAsync(
        string channel,
        string operation,
        Func<CancellationToken, Task> action,
        CancellationToken ct = default)
    {
        await ExecuteAsync<object?>(
            channel,
            operation,
            async innerCt =>
            {
                await action(innerCt);
                return null;
            },
            ct);
    }

    public async Task<T> ExecuteAsync<T>(
        string channel,
        string operation,
        Func<CancellationToken, Task<T>> action,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(channel))
            throw new ArgumentException("Channel is required.", nameof(channel));

        if (string.IsNullOrWhiteSpace(operation))
            throw new ArgumentException("Operation is required.", nameof(operation));

        var policy = ResolvePolicy(channel);
        if (!_options.CurrentValue.Enabled)
            return await action(ct);

        Exception? lastError = null;
        var maxAttempts = Math.Max(1, policy.MaxRetries + 1);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, policy.TimeoutSeconds)));
                return await action(timeoutCts.Token);
            }
            catch (OperationCanceledException) when (!ct.IsCancellationRequested)
            {
                lastError = new TimeoutException($"Outbound integration operation timed out after {policy.TimeoutSeconds} seconds.");
            }
            catch (Exception ex)
            {
                lastError = ex;
            }

            if (attempt >= maxAttempts)
                break;

            var delayMs = policy.ExponentialBackoffEnabled
                ? policy.BaseDelayMilliseconds * (int)Math.Pow(2, attempt - 1)
                : policy.BaseDelayMilliseconds;

            var boundedDelay = Math.Max(50, Math.Min(delayMs, 10_000));
            await Task.Delay(boundedDelay, ct);
        }

        if (lastError is null)
            lastError = new InvalidOperationException("Outbound integration operation failed without an exception.");

        await SaveDeadLetterAsync(channel, operation, maxAttempts, lastError, ct);
        throw lastError;
    }

    public async Task<IReadOnlyList<IntegrationDeadLetterEntry>> GetRecentDeadLettersAsync(int take = 50, CancellationToken ct = default)
    {
        var all = await LoadDeadLettersAsync(ct);
        return all
            .OrderByDescending(x => x.OccurredAtUtc)
            .Take(Math.Clamp(take, 1, 200))
            .ToList();
    }

    public async Task<int> GetDeadLetterCountAsync(CancellationToken ct = default)
    {
        var all = await LoadDeadLettersAsync(ct);
        return all.Count;
    }

    public IntegrationChannelPolicySnapshot GetPolicySnapshot(string channel)
    {
        var policy = ResolvePolicy(channel);
        return new IntegrationChannelPolicySnapshot(
            channel,
            policy.MaxRetries,
            policy.TimeoutSeconds,
            policy.BaseDelayMilliseconds,
            policy.ExponentialBackoffEnabled);
    }

    private IntegrationChannelOptions ResolvePolicy(string channel)
    {
        var options = _options.CurrentValue;
        if (options.Channels.TryGetValue(channel, out var configured) && configured is not null)
            return Sanitize(configured);

        return new IntegrationChannelOptions();
    }

    private static IntegrationChannelOptions Sanitize(IntegrationChannelOptions options)
    {
        return new IntegrationChannelOptions
        {
            MaxRetries = Math.Clamp(options.MaxRetries, 0, 10),
            TimeoutSeconds = Math.Clamp(options.TimeoutSeconds, 1, 300),
            BaseDelayMilliseconds = Math.Clamp(options.BaseDelayMilliseconds, 50, 30_000),
            ExponentialBackoffEnabled = options.ExponentialBackoffEnabled
        };
    }

    private async Task SaveDeadLetterAsync(string channel, string operation, int attempts, Exception ex, CancellationToken ct)
    {
        var entry = new IntegrationDeadLetterEntry(
            Guid.NewGuid(),
            channel,
            operation,
            ex.GetType().Name,
            ex.Message,
            attempts,
            DateTime.UtcNow,
            Guid.NewGuid().ToString("N"));

        _logger.LogError(ex,
            "Outbound integration failed. Channel: {Channel}, Operation: {Operation}, Attempts: {Attempts}, Correlation: {CorrelationId}",
            entry.Channel,
            entry.Operation,
            entry.Attempts,
            entry.CorrelationId);

        await DeadLetterLock.WaitAsync(ct);
        try
        {
            var all = await LoadDeadLettersCoreAsync(ct);
            all.Add(entry);

            var max = Math.Clamp(_options.CurrentValue.MaxDeadLetters, 10, 2000);
            if (all.Count > max)
            {
                all = all
                    .OrderByDescending(x => x.OccurredAtUtc)
                    .Take(max)
                    .ToList();
            }

            await PersistDeadLettersCoreAsync(all, ct);
        }
        finally
        {
            DeadLetterLock.Release();
        }
    }

    private async Task<List<IntegrationDeadLetterEntry>> LoadDeadLettersAsync(CancellationToken ct)
    {
        await DeadLetterLock.WaitAsync(ct);
        try
        {
            return await LoadDeadLettersCoreAsync(ct);
        }
        finally
        {
            DeadLetterLock.Release();
        }
    }

    private async Task<List<IntegrationDeadLetterEntry>> LoadDeadLettersCoreAsync(CancellationToken ct)
    {
        var json = await _cache.GetStringAsync(DeadLetterCacheKey, ct);
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<IntegrationDeadLetterEntry>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private async Task PersistDeadLettersCoreAsync(List<IntegrationDeadLetterEntry> entries, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(entries);
        await _cache.SetStringAsync(
            DeadLetterCacheKey,
            json,
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(14)
            },
            ct);
    }
}
