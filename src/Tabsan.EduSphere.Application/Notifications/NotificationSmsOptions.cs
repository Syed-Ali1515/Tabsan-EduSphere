namespace Tabsan.EduSphere.Application.Notifications;

/// <summary>
/// Configuration options for SMS notification dispatch using Twilio.
/// Bound from appsettings.json:[NotificationSms] section.
/// Example: { "Enabled": true, "PortalUrl": "https://portal.example.com" }
/// </summary>
public sealed class NotificationSmsOptions
{
    public static string SectionName => "NotificationSms";

    /// <summary>When true, SMS dispatch is enabled. When false, SMS notifications are skipped.</summary>
    public bool Enabled { get; set; } = false;

    /// <summary>Portal URL to include in SMS messages for context (optional).</summary>
    public string? PortalUrl { get; set; }
}
