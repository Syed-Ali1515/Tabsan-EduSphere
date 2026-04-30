using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Tabsan.EduSphere.Application.Interfaces;
using Tabsan.EduSphere.Domain.Notifications;
using Tabsan.EduSphere.Infrastructure.Persistence;

namespace Tabsan.EduSphere.Infrastructure.Email;

/// <summary>
/// MailKit-based SMTP email sender.
/// Every send attempt (success or failure) is persisted to the outbound_email_logs table.
/// Configuration keys (all under the "Email" section in appsettings.json):
///   SmtpHost, SmtpPort, Username, Password, FromAddress, FromName, EnableSsl
/// </summary>
public sealed class MailKitEmailSender : IEmailSender
{
    private readonly IConfiguration _config;
    private readonly ILogger<MailKitEmailSender> _logger;
    private readonly ApplicationDbContext _db;

    public MailKitEmailSender(
        IConfiguration config,
        ILogger<MailKitEmailSender> logger,
        ApplicationDbContext db)
    {
        _config = config;
        _logger = logger;
        _db     = db;
    }

    public Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        => SendAsync(to, subject, htmlBody, null, ct);

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        IEnumerable<string>? cc,
        CancellationToken ct = default)
    {
        var section     = _config.GetSection("Email");
        var smtpHost    = section["SmtpHost"]    ?? throw new InvalidOperationException("Email:SmtpHost is not configured.");
        var smtpPort    = int.Parse(section["SmtpPort"] ?? "587");
        var username    = section["Username"]    ?? string.Empty;
        var password    = section["Password"]    ?? string.Empty;
        var fromAddress = section["FromAddress"] ?? throw new InvalidOperationException("Email:FromAddress is not configured.");
        var fromName    = section["FromName"]    ?? "Tabsan EduSphere";
        var enableSsl   = bool.Parse(section["EnableSsl"] ?? "true");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(MailboxAddress.Parse(to));

        if (cc is not null)
        {
            foreach (var addr in cc)
                message.Cc.Add(MailboxAddress.Parse(addr));
        }

        message.Subject = subject;
        message.Body    = new TextPart("html") { Text = htmlBody };

        using var smtp = new SmtpClient();

        try
        {
            var secureSocketOptions = enableSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.None;

            await smtp.ConnectAsync(smtpHost, smtpPort, secureSocketOptions, ct);

            if (!string.IsNullOrEmpty(username))
                await smtp.AuthenticateAsync(username, password, ct);

            await smtp.SendAsync(message, ct);
            await smtp.DisconnectAsync(quit: true, ct);

            _logger.LogInformation(
                "Email sent successfully to {To} subject '{Subject}'",
                to, subject);

            await _db.OutboundEmailLogs.AddAsync(OutboundEmailLog.Sent(to, subject), ct);
            await _db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send email to {To} subject '{Subject}'",
                to, subject);

            // Log the failure before re-throwing so the record is always persisted.
            try
            {
                await _db.OutboundEmailLogs.AddAsync(
                    OutboundEmailLog.Failed(to, subject, ex.Message), ct);
                await _db.SaveChangesAsync(ct);
            }
            catch (Exception dbEx)
            {
                _logger.LogError(dbEx, "Failed to persist email failure log for {To}", to);
            }

            throw;
        }
    }
}
