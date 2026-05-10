using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.Infrastructure.Integrations;

// Default email integration provider using the existing template renderer + SMTP sender.

public sealed class SmtpEmailDeliveryProvider : IEmailDeliveryProvider
{
    private readonly IEmailSender _emailSender;
    private readonly IEmailTemplateRenderer _templates;
    private readonly IOutboundIntegrationGateway _gateway;

    public SmtpEmailDeliveryProvider(
        IEmailSender emailSender,
        IEmailTemplateRenderer templates,
        IOutboundIntegrationGateway gateway)
    {
        _emailSender = emailSender;
        _templates = templates;
        _gateway = gateway;
    }

    public string ProviderKey => "smtp-mailkit";

    public Task SendHtmlAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
        => _gateway.ExecuteAsync(
            channel: "email",
            operation: "smtp.send-html",
            action: gatewayCt => _emailSender.SendAsync(to, subject, htmlBody, gatewayCt),
            ct);

    public Task SendTemplateAsync(string to, string subject, string templateName, IDictionary<string, string> tokens, CancellationToken ct = default)
    {
        var html = _templates.Render(templateName, tokens);
        return _gateway.ExecuteAsync(
            channel: "email",
            operation: "smtp.send-template",
            action: gatewayCt => _emailSender.SendAsync(to, subject, html, gatewayCt),
            ct);
    }
}
