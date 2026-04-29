using Tabsan.EduSphere.Infrastructure.Licensing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Tabsan.EduSphere.BackgroundJobs;

/// <summary>
/// Background hosted service that runs a daily license validity check.
/// On startup it waits for the initial delay, then checks the license every 24 hours.
/// If the license is found to be expired or invalid it logs a warning — the system
/// will automatically enter read-only / degraded mode on the next API request
/// because controllers call LicenseValidationService on the path.
/// </summary>
public class LicenseCheckWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<LicenseCheckWorker> _logger;

    // How often the license check runs. 24 hours in production; can be overridden in tests.
    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public LicenseCheckWorker(IServiceProvider services, ILogger<LicenseCheckWorker> logger)
    {
        _services = services;
        _logger = logger;
    }

    /// <summary>
    /// Main execution loop. Runs once immediately at startup, then every 24 hours.
    /// Uses a new DI scope per iteration so the DbContext is not held open between checks.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("License check worker started.");

        // Initial delay — let the application fully start up first.
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCheckAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("License check worker stopping.");
    }

    /// <summary>
    /// Resolves a scoped LicenseValidationService and performs the check.
    /// Logs the outcome; does not throw — exceptions are caught and logged
    /// to prevent the worker from crashing on transient database errors.
    /// </summary>
    private async Task RunCheckAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _services.CreateScope();
            var licenseService = scope.ServiceProvider.GetRequiredService<LicenseValidationService>();
            var status = await licenseService.ValidateCurrentAsync(ct);
            _logger.LogInformation("Scheduled license check complete. Status={Status}", status);
        }
        catch (OperationCanceledException)
        {
            // Application shutting down — expected, not an error.
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during scheduled license check.");
        }
    }
}
