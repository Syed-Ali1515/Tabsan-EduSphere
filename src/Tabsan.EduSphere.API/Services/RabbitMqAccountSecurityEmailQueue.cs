using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

// Final-Touches Phase 34 Stage 7.2 — RabbitMQ queue producer for account-security email offload path.
public sealed class RabbitMqAccountSecurityEmailQueue : IAccountSecurityEmailQueue, IDisposable
{
    private readonly ILogger<RabbitMqAccountSecurityEmailQueue> _logger;
    private readonly string _queueName;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly object _publishLock = new();

    public RabbitMqAccountSecurityEmailQueue(
        IOptions<QueuePlatformOptions> options,
        ILogger<RabbitMqAccountSecurityEmailQueue> logger)
    {
        _logger = logger;
        var rabbit = options.Value.RabbitMq;

        if (!rabbit.Enabled || string.IsNullOrWhiteSpace(rabbit.ConnectionString))
        {
            throw new InvalidOperationException("QueuePlatform RabbitMq is selected but RabbitMq settings are incomplete.");
        }

        _queueName = string.IsNullOrWhiteSpace(rabbit.AccountSecurityEmailQueueName)
            ? "tabsan.account-security.email"
            : rabbit.AccountSecurityEmailQueueName.Trim();

        var factory = new ConnectionFactory
        {
            Uri = new Uri(rabbit.ConnectionString),
            DispatchConsumersAsync = false
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(_queueName, durable: true, exclusive: false, autoDelete: false);
    }

    public ValueTask EnqueueAsync(AccountSecurityEmailWorkItem workItem, CancellationToken ct = default)
    {
        var payload = JsonSerializer.Serialize(workItem);
        var body = Encoding.UTF8.GetBytes(payload);

        lock (_publishLock)
        {
            var props = _channel.CreateBasicProperties();
            props.Persistent = true;
            _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: props, body: body);
        }

        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        try { _channel.Close(); } catch { }
        try { _connection.Close(); } catch { }
        _channel.Dispose();
        _connection.Dispose();
        _logger.LogInformation("RabbitMQ account-security queue producer disposed.");
    }
}
