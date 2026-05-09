using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

public sealed class ResultPublishJobWorker : BackgroundService
{
    private readonly ResultPublishJobQueue _queue;
    private readonly ResultPublishJobStore _store;
    private readonly IServiceProvider _services;
    private readonly ILogger<ResultPublishJobWorker> _logger;

    public ResultPublishJobWorker(
        ResultPublishJobQueue queue,
        ResultPublishJobStore store,
        IServiceProvider services,
        ILogger<ResultPublishJobWorker> logger)
    {
        _queue = queue;
        _store = store;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                await _store.SetAsync(new ResultPublishJobState
                {
                    JobId = workItem.JobId,
                    CourseOfferingId = workItem.CourseOfferingId,
                    RequestedByUserId = workItem.RequestedByUserId,
                    Status = "running"
                }, stoppingToken);

                using var scope = _services.CreateScope();
                var resultService = scope.ServiceProvider.GetRequiredService<IResultService>();
                var publishedCount = await resultService.PublishAllForOfferingAsync(
                    workItem.CourseOfferingId,
                    workItem.RequestedByUserId,
                    stoppingToken);

                await _store.SetAsync(new ResultPublishJobState
                {
                    JobId = workItem.JobId,
                    CourseOfferingId = workItem.CourseOfferingId,
                    RequestedByUserId = workItem.RequestedByUserId,
                    Status = "completed",
                    PublishedCount = publishedCount,
                    CompletedAt = DateTimeOffset.UtcNow
                }, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Result publish job {JobId} failed.", workItem.JobId);

                await _store.SetAsync(new ResultPublishJobState
                {
                    JobId = workItem.JobId,
                    CourseOfferingId = workItem.CourseOfferingId,
                    RequestedByUserId = workItem.RequestedByUserId,
                    Status = "failed",
                    Error = ex.Message,
                    CompletedAt = DateTimeOffset.UtcNow
                }, stoppingToken);
            }
        }
    }
}