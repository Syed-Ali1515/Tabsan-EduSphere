using Tabsan.EduSphere.Application.Interfaces;
using Microsoft.Extensions.Options;

namespace Tabsan.EduSphere.API.Services;

public sealed class ResultPublishJobWorker : BackgroundService
{
    private readonly ResultPublishJobQueue _queue;
    private readonly ResultPublishJobStore _store;
    private readonly IServiceProvider _services;
    private readonly ILogger<ResultPublishJobWorker> _logger;
    private readonly BackgroundJobReliabilityOptions _reliability;
    private readonly BackgroundJobHealthTracker _healthTracker;

    public ResultPublishJobWorker(
        ResultPublishJobQueue queue,
        ResultPublishJobStore store,
        IServiceProvider services,
        IOptions<BackgroundJobReliabilityOptions> reliability,
        BackgroundJobHealthTracker healthTracker,
        ILogger<ResultPublishJobWorker> logger)
    {
        _queue = queue;
        _store = store;
        _services = services;
        _reliability = reliability.Value;
        _healthTracker = healthTracker;
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

                int publishedCount = 0;
                var maxAttempts = Math.Max(1, _reliability.MaxRetryAttempts);
                for (var attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        using var scope = _services.CreateScope();
                        var resultService = scope.ServiceProvider.GetRequiredService<IResultService>();
                        publishedCount = await resultService.PublishAllForOfferingAsync(
                            workItem.CourseOfferingId,
                            workItem.RequestedByUserId,
                            stoppingToken);
                        break;
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex) when (attempt < maxAttempts)
                    {
                        _healthTracker.RecordResultPublishRetry();
                        var delayMs = Math.Max(25, _reliability.BaseDelayMilliseconds * attempt);
                        _logger.LogWarning(ex, "Result publish job {JobId} attempt {Attempt}/{MaxAttempts} failed, retrying in {DelayMs}ms.", workItem.JobId, attempt, maxAttempts, delayMs);
                        await Task.Delay(TimeSpan.FromMilliseconds(delayMs), stoppingToken);
                    }
                }

                await _store.SetAsync(new ResultPublishJobState
                {
                    JobId = workItem.JobId,
                    CourseOfferingId = workItem.CourseOfferingId,
                    RequestedByUserId = workItem.RequestedByUserId,
                    Status = "completed",
                    PublishedCount = publishedCount,
                    CompletedAt = DateTimeOffset.UtcNow
                }, stoppingToken);
                _healthTracker.RecordResultPublishSuccess();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Result publish job {JobId} failed.", workItem.JobId);
                _healthTracker.RecordResultPublishFailure();
                var consecutiveFailures = _healthTracker.GetResultPublishConsecutiveFailures();
                if (consecutiveFailures >= Math.Max(1, _reliability.AlertConsecutiveFailureThreshold))
                {
                    _logger.LogWarning("Result publish worker consecutive failures reached {ConsecutiveFailures}.", consecutiveFailures);
                }

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