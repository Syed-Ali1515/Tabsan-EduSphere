using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.BackgroundJobs;

/// <summary>
/// Background hosted service that runs a daily check for upcoming license expiry.
/// Sends a system notification to all Admin and SuperAdmin users when the active license
/// is within <see cref="WarningThresholdDays"/> days of expiring.
///
/// The job runs once at startup (after a short delay) and then every 24 hours.
/// </summary>
public class LicenseExpiryWarningJob : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<LicenseExpiryWarningJob> _logger;

    /// <summary>Days before expiry at which warning notifications are sent.</summary>
    private const int WarningThresholdDays = 5;

    private readonly TimeSpan _interval = TimeSpan.FromHours(24);

    public LicenseExpiryWarningJob(
        IServiceProvider services,
        ILogger<LicenseExpiryWarningJob> logger)
    {
        _services = services;
        _logger   = logger;
    }

    /// <summary>
    /// Main execution loop. Waits 60 s after startup, then runs once per 24 h.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LicenseExpiryWarningJob started.");
        await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCheckAsync(stoppingToken);
            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("LicenseExpiryWarningJob stopping.");
    }

    /// <summary>
    /// Resolves a DI scope, queries the current license state, and sends a warning
    /// notification to Admin/SuperAdmin users if the license expires within the threshold.
    /// Permanent licenses (null ExpiresAt) are skipped.
    /// </summary>
    private async Task RunCheckAsync(CancellationToken ct)
    {
        try
        {
            using var scope = _services.CreateScope();
            var db          = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notifSvc    = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var license = await db.LicenseStates
                                  .OrderByDescending(l => l.ActivatedAt)
                                  .FirstOrDefaultAsync(ct);

            if (license is null || !license.ExpiresAt.HasValue)
            {
                _logger.LogDebug("LicenseExpiryWarningJob: no expiring license found.");
                return;
            }

            var daysLeft = (license.ExpiresAt.Value - DateTime.UtcNow).TotalDays;

            if (daysLeft > WarningThresholdDays)
            {
                _logger.LogDebug(
                    "LicenseExpiryWarningJob: license expires in {Days:F1} days — no warning needed.",
                    daysLeft);
                return;
            }

            // Collect Admin + SuperAdmin user IDs to receive the notification
            var recipientIds = await db.Users
                .Where(u => u.Role.Name == "Admin" || u.Role.Name == "SuperAdmin")
                .Select(u => u.Id)
                .ToListAsync(ct);

            if (recipientIds.Count == 0)
            {
                _logger.LogWarning("LicenseExpiryWarningJob: no Admin/SuperAdmin users found to notify.");
                return;
            }

            var expiryLabel = daysLeft <= 0
                ? "has already expired"
                : $"expires in {(int)Math.Ceiling(daysLeft)} day(s) on {license.ExpiresAt.Value:yyyy-MM-dd}";

            await notifSvc.SendSystemAsync(
                title:            "License Expiry Warning",
                body:             $"The Tabsan EduSphere license {expiryLabel}. Please upload a new .tablic license file to avoid service interruption.",
                type:             NotificationType.System,
                recipientUserIds: recipientIds,
                ct:               ct);

            _logger.LogInformation(
                "LicenseExpiryWarningJob: sent warning to {Count} recipient(s). Days remaining: {Days:F1}",
                recipientIds.Count, daysLeft);
        }
        catch (OperationCanceledException) { /* application shutting down */ }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LicenseExpiryWarningJob encountered an error during the check.");
        }
    }
}
