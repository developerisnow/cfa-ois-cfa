using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OIS.Settlement.Infrastructure;
using Polly;

namespace OIS.Settlement.Background;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OutboxPublisher> _logger;
    private readonly IConfiguration _configuration;

    public OutboxPublisher(IServiceProvider services, ILogger<OutboxPublisher> logger, IConfiguration configuration)
    {
        _services = services;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _configuration.GetValue<bool>("Kafka:Enabled", true);
        if (!enabled)
        {
            _logger.LogWarning("Kafka disabled, OutboxPublisher will not start");
            return;
        }

        var bootstrap = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        var producerConfig = new ProducerConfig { BootstrapServers = bootstrap };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SettlementDbContext>();

                var messages = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                if (messages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
                foreach (var msg in messages)
                {
                    var retry = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                            (ex, ts, attempt, ctx) => _logger.LogWarning(ex, "Outbox publish retry {Attempt} after {Delay}ms", attempt, ts.TotalMilliseconds));

                    await retry.ExecuteAsync(async () =>
                    {
                        await producer.ProduceAsync(msg.Topic, new Message<string, string>
                        {
                            Key = msg.Id.ToString(),
                            Value = msg.Payload
                        }, stoppingToken);
                    });

                    msg.ProcessedAt = DateTime.UtcNow;
                    Metrics.OutboxPublished.Add(1, new("topic", msg.Topic));
                }

                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisher cycle failed");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}

