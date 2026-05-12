using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tabsan.EduSphere.Infrastructure.Persistence;
using System.Net;

namespace Tabsan.EduSphere.API.Services;

// Final-Touches Phase 9 Stage 9.1 — shared request/health observability state for Prometheus and SLO summaries.
public sealed class ObservabilityMetrics
{
    private readonly object _gate = new();
    private readonly Queue<double> _requestDurationsMs = new();
    private readonly int _sampleLimit;
    private long _totalRequests;
    private long _errorResponses;

    public ObservabilityMetrics(DateTimeOffset processStartUtc, int sampleLimit = 4096)
    {
        ProcessStartUtc = processStartUtc;
        _sampleLimit = Math.Max(128, sampleLimit);
    }

    public DateTimeOffset ProcessStartUtc { get; }

    public static string MeterName => "Tabsan.EduSphere.API";

    public void RecordRequest(TimeSpan duration, int statusCode)
    {
        var durationMs = duration.TotalMilliseconds;
        var isError = statusCode >= 500;

        lock (_gate)
        {
            _totalRequests++;
            if (isError)
            {
                _errorResponses++;
            }

            _requestDurationsMs.Enqueue(durationMs);
            while (_requestDurationsMs.Count > _sampleLimit)
            {
                _requestDurationsMs.Dequeue();
            }
        }
    }

    public ObservabilitySnapshot GetSnapshot()
    {
        lock (_gate)
        {
            var samples = _requestDurationsMs.ToArray();
            Array.Sort(samples);

            return new ObservabilitySnapshot(
                TotalRequests: _totalRequests,
                ErrorResponses: _errorResponses,
                ErrorRatePercent: _totalRequests == 0 ? 0 : (_errorResponses * 100.0 / _totalRequests),
                P50Milliseconds: Percentile(samples, 0.50),
                P95Milliseconds: Percentile(samples, 0.95),
                P99Milliseconds: Percentile(samples, 0.99),
                SampleCount: samples.Length,
                ProcessCpuPercent: GetAverageCpuPercent(ProcessStartUtc),
                WorkingSetBytes: Process.GetCurrentProcess().WorkingSet64,
                TotalAllocatedBytes: GC.GetTotalMemory(forceFullCollection: false));
        }
    }

    private static double Percentile(double[] samples, double percentile)
    {
        if (samples.Length == 0)
        {
            return 0;
        }

        if (samples.Length == 1)
        {
            return samples[0];
        }

        var rank = percentile * (samples.Length - 1);
        var lowerIndex = (int)Math.Floor(rank);
        var upperIndex = (int)Math.Ceiling(rank);

        if (lowerIndex == upperIndex)
        {
            return samples[lowerIndex];
        }

        var fraction = rank - lowerIndex;
        return samples[lowerIndex] + ((samples[upperIndex] - samples[lowerIndex]) * fraction);
    }

    private static double GetAverageCpuPercent(DateTimeOffset processStartUtc)
    {
        var elapsedSeconds = Math.Max(1, (DateTimeOffset.UtcNow - processStartUtc).TotalSeconds);
        var cpuSeconds = Process.GetCurrentProcess().TotalProcessorTime.TotalSeconds;
        return (cpuSeconds / elapsedSeconds) / Environment.ProcessorCount * 100.0;
    }
}

public sealed record ObservabilitySnapshot(
    long TotalRequests,
    long ErrorResponses,
    double ErrorRatePercent,
    double P50Milliseconds,
    double P95Milliseconds,
    double P99Milliseconds,
    int SampleCount,
    double ProcessCpuPercent,
    long WorkingSetBytes,
    long TotalAllocatedBytes);

// Final-Touches Phase 9 Stage 9.3 — database connectivity health for runtime observability.
public sealed class DatabaseConnectivityHealthCheck(ApplicationDbContext dbContext) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy("Database connectivity is healthy.")
                : HealthCheckResult.Unhealthy("Database connectivity failed.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database connectivity check failed.", ex);
        }
    }
}

// Final-Touches Phase 9 Stage 9.3 — memory-pressure health for continuous host monitoring.
public sealed class MemoryPressureHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var thresholdPercent = Math.Clamp(configuration.GetValue("InfrastructureTuning:Monitoring:MemoryThresholdPercent", 85), 1, 99);
        var gcInfo = GC.GetGCMemoryInfo();
        var totalAvailableBytes = gcInfo.TotalAvailableMemoryBytes;
        var allocatedBytes = GC.GetTotalMemory(forceFullCollection: false);
        var pressurePercent = totalAvailableBytes > 0
            ? allocatedBytes * 100.0 / totalAvailableBytes
            : 0;

        return Task.FromResult(pressurePercent <= thresholdPercent
            ? HealthCheckResult.Healthy($"Memory pressure is within threshold ({pressurePercent:F1}% <= {thresholdPercent}%).")
            : HealthCheckResult.Unhealthy($"Memory pressure exceeded threshold ({pressurePercent:F1}% > {thresholdPercent}%)."));
    }
}

// Final-Touches Phase 9 Stage 9.3 — CPU-pressure health for continuous host monitoring.
public sealed class CpuPressureHealthCheck(ObservabilityMetrics metrics, IConfiguration configuration) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var thresholdPercent = Math.Clamp(configuration.GetValue("InfrastructureTuning:Monitoring:CpuThresholdPercent", 85), 1, 99);
        var snapshot = metrics.GetSnapshot();

        return Task.FromResult(snapshot.ProcessCpuPercent <= thresholdPercent
            ? HealthCheckResult.Healthy($"CPU pressure is within threshold ({snapshot.ProcessCpuPercent:F1}% <= {thresholdPercent}%).")
            : HealthCheckResult.Unhealthy($"CPU pressure exceeded threshold ({snapshot.ProcessCpuPercent:F1}% > {thresholdPercent}%)."));
    }
}

// Final-Touches Phase 9 Stage 9.3 — network-responsiveness health based on DNS resolution of the configured public endpoint.
public sealed class NetworkStackHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var probeUrl = configuration["InfrastructureTuning:Monitoring:NetworkProbeUrl"]
            ?? configuration["AppSettings:WebBaseUrl"]
            ?? configuration["EduApi:BaseUrl"]
            ?? "http://localhost";

        if (!Uri.TryCreate(probeUrl, UriKind.Absolute, out var uri) || string.IsNullOrWhiteSpace(uri.Host))
        {
            return HealthCheckResult.Degraded($"Network probe URL '{probeUrl}' is not a valid absolute URI.");
        }

        try
        {
            await Dns.GetHostEntryAsync(uri.Host);
            return HealthCheckResult.Healthy($"Network resolution is healthy for {uri.Host}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Network resolution failed for {uri.Host}.", ex);
        }
    }
}

// Final-Touches Phase 9 Stage 9.3 — request-error health based on rolling error-rate telemetry.
public sealed class ErrorRateHealthCheck(ObservabilityMetrics metrics, IConfiguration configuration) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var thresholdPercent = Math.Clamp(configuration.GetValue("InfrastructureTuning:Monitoring:ErrorRateThresholdPercent", 5.0), 0.1, 100.0);
        var snapshot = metrics.GetSnapshot();

        return Task.FromResult(snapshot.ErrorRatePercent <= thresholdPercent
            ? HealthCheckResult.Healthy($"Error rate is within threshold ({snapshot.ErrorRatePercent:F2}% <= {thresholdPercent:F2}%).")
            : HealthCheckResult.Unhealthy($"Error rate exceeded threshold ({snapshot.ErrorRatePercent:F2}% > {thresholdPercent:F2}%)."));
    }
}
