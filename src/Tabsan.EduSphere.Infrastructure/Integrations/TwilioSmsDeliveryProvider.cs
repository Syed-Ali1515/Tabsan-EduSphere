using Microsoft.Extensions.Logging;
using Tabsan.EduSphere.Application.Interfaces;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Tabsan.EduSphere.Infrastructure.Integrations;

/// <summary>
/// Twilio SMS delivery provider using the free tier.
/// Requires TWILIO_ACCOUNT_SID, TWILIO_AUTH_TOKEN, and TWILIO_PHONE_NUMBER environment variables.
/// Integrates with the resilient outbound gateway for circuit breaking and retries.
/// </summary>
public sealed class TwilioSmsDeliveryProvider : ISmsDeliveryProvider
{
    private readonly IOutboundIntegrationGateway _gateway;
    private readonly ILogger<TwilioSmsDeliveryProvider> _logger;
    private readonly string? _twilioAccountSid;
    private readonly string? _twilioAuthToken;
    private readonly string? _twilioPhoneNumber;

    public TwilioSmsDeliveryProvider(
        IOutboundIntegrationGateway gateway,
        ILogger<TwilioSmsDeliveryProvider>? logger = null)
    {
        _gateway = gateway;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<TwilioSmsDeliveryProvider>.Instance;

        // Load from environment variables
        _twilioAccountSid = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
        _twilioAuthToken = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");
        _twilioPhoneNumber = Environment.GetEnvironmentVariable("TWILIO_PHONE_NUMBER");
    }

    public string ProviderKey => "twilio-sms";

    public Task SendAsync(string to, string body, CancellationToken ct = default)
        => _gateway.ExecuteAsync(
            channel: "sms",
            operation: "twilio.send",
            action: gatewayCt => SendInternalAsync(to, body, gatewayCt),
            ct);

    public Task SendTemplateAsync(string to, string templateName, IDictionary<string, string> tokens, CancellationToken ct = default)
    {
        // Render SMS body from tokens — SMS templates typically have fewer tokens than email
        var body = RenderSmsTemplate(templateName, tokens);
        return SendAsync(to, body, ct);
    }

    private async Task SendInternalAsync(string to, string body, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_twilioAccountSid) ||
            string.IsNullOrWhiteSpace(_twilioAuthToken) ||
            string.IsNullOrWhiteSpace(_twilioPhoneNumber))
        {
            throw new InvalidOperationException(
                "Twilio SMS provider is not configured. Please set TWILIO_ACCOUNT_SID, TWILIO_AUTH_TOKEN, and TWILIO_PHONE_NUMBER environment variables.");
        }

        TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);

        // Validate phone number format (basic E.164 check)
        if (!to.StartsWith("+") || !to.Skip(1).All(char.IsDigit))
        {
            throw new ArgumentException($"Invalid phone number format: {to}. Expected E.164 format (e.g., +1234567890).");
        }

        var message = await MessageResource.CreateAsync(
            body: body,
            from: new PhoneNumber(_twilioPhoneNumber),
            to: new PhoneNumber(to));

        _logger.LogInformation("SMS sent successfully. MessageSid={MessageSid}, To={To}", message.Sid, to);
    }

    private static string RenderSmsTemplate(string templateName, IDictionary<string, string> tokens)
    {
        // Simple SMS template mapping — SMS messages are plain text and short
        return templateName switch
        {
            "notification-alert" => RenderNotificationAlertSms(tokens),
            _ => throw new ArgumentException($"Unknown SMS template: {templateName}")
        };
    }

    private static string RenderNotificationAlertSms(IDictionary<string, string> tokens)
    {
        tokens.TryGetValue("TITLE", out var title);
        tokens.TryGetValue("BODY", out var body);
        tokens.TryGetValue("TYPE", out var type);

        // SMS: Keep it concise — typical limit is 160 characters for a single segment
        var message = $"[{title}] {body}";
        
        if (!string.IsNullOrEmpty(type))
        {
            message = $"{message} ({type})";
        }

        // Truncate to SMS segment size if needed (160 chars for single segment, 153 for multipart)
        if (message.Length > 160)
        {
            message = message.Substring(0, 157) + "...";
        }

        return message;
    }
}
