using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tabsan.EduSphere.Infrastructure.Integrations;

namespace Tabsan.EduSphere.UnitTests;

// Final-Touches Phase 30 Stage 30.1 - resilient integration gateway tests.
public class Phase30Stage1Tests
{
    [Fact]
    public async Task ExecuteAsync_ShouldRetryAndSucceed_OnTransientFailure()
    {
        var sut = BuildGateway(new IntegrationGatewayOptions
        {
            Enabled = true,
            Channels = new Dictionary<string, IntegrationChannelOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["email"] = new() { MaxRetries = 2, TimeoutSeconds = 5, BaseDelayMilliseconds = 50, ExponentialBackoffEnabled = false }
            }
        });

        var attempts = 0;
        await sut.ExecuteAsync(
            channel: "email",
            operation: "smtp.send-html",
            action: _ =>
            {
                attempts++;
                if (attempts < 2)
                    throw new InvalidOperationException("Transient failure");

                return Task.CompletedTask;
            });

        attempts.Should().Be(2);
        (await sut.GetDeadLetterCountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldAddDeadLetter_WhenAllAttemptsFail()
    {
        var sut = BuildGateway(new IntegrationGatewayOptions
        {
            Enabled = true,
            Channels = new Dictionary<string, IntegrationChannelOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["lms-external-api"] = new() { MaxRetries = 1, TimeoutSeconds = 1, BaseDelayMilliseconds = 50, ExponentialBackoffEnabled = false }
            }
        });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.ExecuteAsync(
                channel: "lms-external-api",
                operation: "library.loan-lookup",
                action: _ => throw new InvalidOperationException("External API unavailable")));

        var deadLetters = await sut.GetRecentDeadLettersAsync();
        deadLetters.Should().HaveCount(1);
        deadLetters[0].Channel.Should().Be("lms-external-api");
        deadLetters[0].Operation.Should().Be("library.loan-lookup");
        deadLetters[0].Attempts.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldConvertGatewayTimeoutToDeadLetter()
    {
        var sut = BuildGateway(new IntegrationGatewayOptions
        {
            Enabled = true,
            Channels = new Dictionary<string, IntegrationChannelOptions>(StringComparer.OrdinalIgnoreCase)
            {
                ["push"] = new() { MaxRetries = 0, TimeoutSeconds = 1, BaseDelayMilliseconds = 50, ExponentialBackoffEnabled = false }
            }
        });

        await Assert.ThrowsAsync<TimeoutException>(() =>
            sut.ExecuteAsync(
                channel: "push",
                operation: "announcement.broadcast",
                action: async ct =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), ct);
                }));

        var deadLetters = await sut.GetRecentDeadLettersAsync();
        deadLetters.Should().ContainSingle();
        deadLetters[0].ErrorType.Should().Be(nameof(TimeoutException));
    }

    [Fact]
    public void GetPolicySnapshot_ShouldUseDefault_WhenChannelMissing()
    {
        var sut = BuildGateway(new IntegrationGatewayOptions { Enabled = true });

        var policy = sut.GetPolicySnapshot("unknown-channel");

        policy.MaxRetries.Should().Be(2);
        policy.TimeoutSeconds.Should().Be(15);
        policy.BaseDelayMilliseconds.Should().Be(200);
    }

    private static ResilientOutboundIntegrationGateway BuildGateway(IntegrationGatewayOptions options)
    {
        return new ResilientOutboundIntegrationGateway(
            new InMemoryDistributedCache(),
            new StaticOptionsMonitor<IntegrationGatewayOptions>(options),
            NullLogger<ResilientOutboundIntegrationGateway>.Instance);
    }
}

file sealed class StaticOptionsMonitor<T> : IOptionsMonitor<T>
{
    public StaticOptionsMonitor(T current) => CurrentValue = current;

    public T CurrentValue { get; }

    public T Get(string? name) => CurrentValue;

    public IDisposable? OnChange(Action<T, string?> listener) => null;
}

file sealed class InMemoryDistributedCache : IDistributedCache
{
    private readonly ConcurrentDictionary<string, byte[]> _entries = new(StringComparer.OrdinalIgnoreCase);

    public byte[]? Get(string key)
        => _entries.TryGetValue(key, out var value) ? value : null;

    public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        => Task.FromResult(Get(key));

    public void Refresh(string key)
    {
    }

    public Task RefreshAsync(string key, CancellationToken token = default)
        => Task.CompletedTask;

    public void Remove(string key)
        => _entries.TryRemove(key, out _);

    public Task RemoveAsync(string key, CancellationToken token = default)
    {
        Remove(key);
        return Task.CompletedTask;
    }

    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        => _entries[key] = value;

    public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
    {
        Set(key, value, options);
        return Task.CompletedTask;
    }
}
