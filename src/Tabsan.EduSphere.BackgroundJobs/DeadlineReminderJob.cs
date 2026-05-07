using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.BackgroundJobs;

/// <summary>
/// Background hosted service that runs daily to dispatch reminder notifications
/// for upcoming academic deadlines whose reminder window has arrived.
///
/// Configuration (appsettings.json → DeadlineReminder):
///   IntervalHours — how often the job runs in hours (default 24)
/// </summary>
public class DeadlineReminderJob : BackgroundService
{
    private readonly IServiceProvider            _services;
    private readonly ILogger<DeadlineReminderJob> _logger;
    private readonly TimeSpan                    _interval;

    public DeadlineReminderJob(
        IServiceProvider services,
        ILogger<DeadlineReminderJob> logger,
        IConfiguration config)
    {
        _services = services;
        _logger   = logger;
        _interval = TimeSpan.FromHours(config.GetValue<double>("DeadlineReminder:IntervalHours", 24));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DeadlineReminderJob started (interval={Interval}h).", _interval.TotalHours);

        // Brief startup delay so the host is fully initialised before the first run.
        await Task.Delay(TimeSpan.FromSeconds(90), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCheckAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("DeadlineReminderJob stopping.");
    }

    private async Task RunCheckAsync(CancellationToken ct)
    {
        try
        {
            using var scope   = _services.CreateScope();
            var calendarSvc   = scope.ServiceProvider.GetRequiredService<IAcademicCalendarService>();
            int dispatched    = await calendarSvc.DispatchPendingRemindersAsync(ct);

            if (dispatched > 0)
                _logger.LogInformation("DeadlineReminderJob: dispatched {Count} reminder notification(s).", dispatched);
            else
                _logger.LogDebug("DeadlineReminderJob: no pending reminders.");
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { /* shutting down */ }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DeadlineReminderJob encountered an error during RunCheckAsync.");
        }
    }
}
