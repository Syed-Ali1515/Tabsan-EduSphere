using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Tabsan.EduSphere.API.Services;

public sealed class ReportExportJobStore
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan JobTtl = TimeSpan.FromHours(24);

    public ReportExportJobStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SetStateAsync(ReportExportJobState state, CancellationToken ct)
    {
        await _cache.SetStringAsync(
            GetStateKey(state.JobId),
            JsonSerializer.Serialize(state),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = JobTtl },
            ct);
    }

    public async Task<ReportExportJobState?> GetStateAsync(Guid jobId, CancellationToken ct)
    {
        var raw = await _cache.GetStringAsync(GetStateKey(jobId), ct);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return JsonSerializer.Deserialize<ReportExportJobState>(raw);
    }

    public async Task SetPayloadAsync(Guid jobId, byte[] bytes, CancellationToken ct)
    {
        await _cache.SetAsync(
            GetPayloadKey(jobId),
            bytes,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = JobTtl },
            ct);
    }

    public Task<byte[]?> GetPayloadAsync(Guid jobId, CancellationToken ct)
        => _cache.GetAsync(GetPayloadKey(jobId), ct);

    private static string GetStateKey(Guid jobId) => $"report-export-job:state:{jobId}";
    private static string GetPayloadKey(Guid jobId) => $"report-export-job:payload:{jobId}";
}