using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

// Final-Touches Phase 34 Stage 7.1 — background worker for account-security email queue processing.
public sealed class InMemoryAccountSecurityEmailWorker : BackgroundService
{
    private readonly InMemoryAccountSecurityEmailQueue _queue;
    private readonly IServiceProvider _services;
    private readonly ILogger<InMemoryAccountSecurityEmailWorker> _logger;

    public InMemoryAccountSecurityEmailWorker(
        InMemoryAccountSecurityEmailQueue queue,
        IServiceProvider services,
        ILogger<InMemoryAccountSecurityEmailWorker> logger)
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
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                await emailSender.SendAsync(workItem.To, workItem.Subject, workItem.HtmlBody, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Account security email send failed in in-memory worker. Reason={Reason} To={To}",
                    workItem.Reason,
                    workItem.To);
            }
        }
    }
}
