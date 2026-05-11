namespace Tabsan.EduSphere.API.Services;

// Final-Touches Phase 34 Stage 7.2 — deployment-model queue platform selection.
public sealed class QueuePlatformOptions
{
    public string Provider { get; set; } = "InMemory";
    public RabbitMqQueuePlatformOptions RabbitMq { get; set; } = new();
}

public sealed class RabbitMqQueuePlatformOptions
{
    public bool Enabled { get; set; }
    public string ConnectionString { get; set; } = "";
    public string AccountSecurityEmailQueueName { get; set; } = "tabsan.account-security.email";
    public int PollDelayMilliseconds { get; set; } = 400;
}
