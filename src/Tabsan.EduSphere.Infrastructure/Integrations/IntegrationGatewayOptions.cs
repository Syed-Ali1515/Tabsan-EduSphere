namespace Tabsan.EduSphere.Infrastructure.Integrations;

// Final-Touches Phase 30 Stage 30.1 - configurable retry/timeout policy per integration channel.
public sealed class IntegrationGatewayOptions
{
    public bool Enabled { get; set; } = true;
    public int MaxDeadLetters { get; set; } = 200;
    public Dictionary<string, IntegrationChannelOptions> Channels { get; set; } = new(StringComparer.OrdinalIgnoreCase)
    {
        ["payment"] = new IntegrationChannelOptions { MaxRetries = 2, TimeoutSeconds = 20, BaseDelayMilliseconds = 300, ExponentialBackoffEnabled = true },
        ["email"] = new IntegrationChannelOptions { MaxRetries = 2, TimeoutSeconds = 20, BaseDelayMilliseconds = 250, ExponentialBackoffEnabled = true },
        ["sms"] = new IntegrationChannelOptions { MaxRetries = 2, TimeoutSeconds = 15, BaseDelayMilliseconds = 200, ExponentialBackoffEnabled = true },
        ["push"] = new IntegrationChannelOptions { MaxRetries = 1, TimeoutSeconds = 10, BaseDelayMilliseconds = 150, ExponentialBackoffEnabled = true },
        ["lms-external-api"] = new IntegrationChannelOptions { MaxRetries = 2, TimeoutSeconds = 15, BaseDelayMilliseconds = 200, ExponentialBackoffEnabled = true }
    };
}

public sealed class IntegrationChannelOptions
{
    public int MaxRetries { get; set; } = 2;
    public int TimeoutSeconds { get; set; } = 15;
    public int BaseDelayMilliseconds { get; set; } = 200;
    public bool ExponentialBackoffEnabled { get; set; } = true;
}
