using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Tabsan.EduSphere.Application.Interfaces;

namespace Tabsan.EduSphere.API.Services;

// Final-Touches Phase 34 Stage 7.2 — RabbitMQ queue consumer for account-security email offload path.
public sealed class RabbitMqAccountSecurityEmailWorker : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RabbitMqAccountSecurityEmailWorker> _logger;
    private readonly RabbitMqQueuePlatformOptions _options;

    public RabbitMqAccountSecurityEmailWorker(
        IServiceProvider services,
        IOptions<QueuePlatformOptions> options,
        ILogger<RabbitMqAccountSecurityEmailWorker> logger)
    {
        _services = services;
        _logger = logger;
        _options = options.Value.RabbitMq;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            _logger.LogWarning("RabbitMQ account-security worker is configured but RabbitMQ settings are incomplete. Worker will stop.");
            return;
        }

        var queueName = string.IsNullOrWhiteSpace(_options.AccountSecurityEmailQueueName)
            ? "tabsan.account-security.email"
            : _options.AccountSecurityEmailQueueName.Trim();

        var pollDelay = TimeSpan.FromMilliseconds(Math.Clamp(_options.PollDelayMilliseconds, 50, 5000));

        var factory = new ConnectionFactory
        {
            Uri = new Uri(_options.ConnectionString),
            DispatchConsumersAsync = false
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queueName, durable: true, exclusive: false, autoDelete: false);

        while (!stoppingToken.IsCancellationRequested)
        {
            BasicGetResult? delivery = null;
            try
            {
                delivery = channel.BasicGet(queueName, autoAck: false);
                if (delivery is null)
                {
                    await Task.Delay(pollDelay, stoppingToken);
                    continue;
                }

                var payload = Encoding.UTF8.GetString(delivery.Body.ToArray());
                var workItem = JsonSerializer.Deserialize<AccountSecurityEmailWorkItem>(payload);
                if (workItem is null)
                {
                    channel.BasicAck(delivery.DeliveryTag, multiple: false);
                    continue;
                }

                using var scope = _services.CreateScope();
                var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();
                await emailSender.SendAsync(workItem.To, workItem.Subject, workItem.HtmlBody, stoppingToken);

                channel.BasicAck(delivery.DeliveryTag, multiple: false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "RabbitMQ account-security email processing failed.");
                if (delivery is not null)
                {
                    channel.BasicNack(delivery.DeliveryTag, multiple: false, requeue: false);
                }
            }
        }
    }
}
