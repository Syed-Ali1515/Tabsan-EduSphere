using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Integrations;

// Default email integration provider using the existing template renderer + SMTP sender.

public sealed class SmtpEmailDeliveryProvider : IEmailDeliveryProvider
{
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templates;

    public SmtpEmailDeliveryProvider(IEmailSender emailSender, IEmailTemplateRenderer templates)
    {
        _emailSender = emailSender;
        _templates = templates;
    }

    public string ProviderKey => "smtp-mailkit";

    public Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        => _emailSender.SendAsync(to, subject, htmlBody, ct);

    public Task SendTemplateAsync(string to, string subject, string templateName, IDictionary<string, string> tokens, CancellationToken ct = default)
    {
        var html = _templates.Render(templateName, tokens);
        return _emailSender.SendAsync(to, subject, html, ct);
    }
}
