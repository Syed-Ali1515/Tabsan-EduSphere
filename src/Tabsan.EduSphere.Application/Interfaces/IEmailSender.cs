namespace Tabsan.EduSphere.Application.Interfaces;

/// <summary>
/// Application-layer contract for sending transactional emails.
/// Implementations are provided by the Infrastructure layer (MailKit SMTP).
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an HTML email to a single recipient.
    /// </summary>
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);

    /// <summary>
    /// Sends an HTML email with optional CC recipients.
    /// </summary>
    Task SendAsync(string to, string subject, string htmlBody, IEnumerable<string>? cc, CancellationToken ct = default);
}
