using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Tabsan.EduSphere.API.Services;

public sealed class ResultPublishJobStore
{
    private readonly IDistributedCache _cache;
    private static readonly TimeSpan JobTtl = TimeSpan.FromHours(24);

    public ResultPublishJobStore(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task SetAsync(ResultPublishJobState state, CancellationToken ct)
    {
        await _cache.SetStringAsync(
            GetKey(state.JobId),
            JsonSerializer.Serialize(state),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = JobTtl },
            ct);
    }

    public async Task<ResultPublishJobState?> GetAsync(Guid jobId, CancellationToken ct)
    {
        var raw = await _cache.GetStringAsync(GetKey(jobId), ct);
        if (string.IsNullOrWhiteSpace(raw))
            return null;

        return JsonSerializer.Deserialize<ResultPublishJobState>(raw);
    }

    private static string GetKey(Guid jobId) => $"result-publish-job:{jobId}";
}