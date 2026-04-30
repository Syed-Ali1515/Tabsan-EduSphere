namespace Tabsan.EduSphere.Domain.Notifications;

/// <summary>
/// Append-only log of every outbound email attempt made by the system.
/// Provides an audit trail of sent / failed messages for operational monitoring.
/// </summary>
public class OutboundEmailLog
{
    /// <summary>Primary key.</summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>Recipient address.</summary>
    public string ToAddress { get; private set; } = default!;

    /// <summary>Email subject line.</summary>
    public string Subject { get; private set; } = default!;

    /// <summary>"Sent" or "Failed".</summary>
    public string Status { get; private set; } = default!;

    /// <summary>Exception message when Status is "Failed". Null on success.</summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>UTC timestamp when the send attempt was made.</summary>
    public DateTime AttemptedAt { get; private set; }

    private OutboundEmailLog() { }

    /// <summary>Creates a successful send record.</summary>
    public static OutboundEmailLog Sent(string to, string subject) =>
        new()
        {
            ToAddress   = to,
            Subject     = subject,
            Status      = "Sent",
            AttemptedAt = DateTime.UtcNow
        };

    /// <summary>Creates a failed send record including the error reason.</summary>
    public static OutboundEmailLog Failed(string to, string subject, string errorMessage) =>
        new()
        {
            ToAddress    = to,
            Subject      = subject,
            Status       = "Failed",
            ErrorMessage = errorMessage,
            AttemptedAt  = DateTime.UtcNow
        };
}
