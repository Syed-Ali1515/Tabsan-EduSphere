using Microsoft.Extensions.DependencyInjection;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;

namespace Tabsan.EduSphere.API.Services;

public sealed class NotificationFanoutWorker : BackgroundService
{
    private readonly NotificationFanoutQueue _queue;
    private readonly IServiceProvider _services;
    private readonly ILogger<NotificationFanoutWorker> _logger;

    public NotificationFanoutWorker(
        NotificationFanoutQueue queue,
        IServiceProvider services,
        ILogger<NotificationFanoutWorker> logger)
    {
        _queue = queue;
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var workItem in _queue.DequeueAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _services.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                const int batchSize = 500;
                foreach (var batch in workItem.RecipientUserIds.Distinct().Chunk(batchSize))
                {
                    var recipients = batch.Select(userId => new NotificationRecipient(workItem.NotificationId, userId));
                    await repo.AddRecipientsAsync(recipients, stoppingToken);
                    await repo.SaveChangesAsync(stoppingToken);
                }

                _logger.LogInformation(
                    "Notification fan-out worker processed notification {NotificationId} for {RecipientCount} recipients.",
                    workItem.NotificationId,
                    workItem.RecipientUserIds.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Notification fan-out worker failed for notification {NotificationId}.", workItem.NotificationId);
            }
        }
    }
}