using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OIS.Contracts.Events;
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
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<SettlementDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

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

                foreach (var msg in messages)
                {
                    var retry = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                            (ex, ts, attempt, ctx) => _logger.LogWarning(ex, "Outbox publish retry {Attempt} after {Delay}ms", attempt, ts.TotalMilliseconds));

                    await retry.ExecuteAsync(async () =>
                    {
                        await PublishTypedAsync(publisher, msg, stoppingToken);
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

    private static async Task PublishTypedAsync(IPublishEndpoint publisher, OutboxMessage msg, CancellationToken ct)
    {
        // Map topic to contract and publish typed; fallback to raw AuditLogged
        switch (msg.Topic)
        {
            case "ois.payout.executed":
            {
                var data = System.Text.Json.JsonSerializer.Deserialize<PayoutExecuted>(msg.Payload);
                if (data != null)
                {
                    await publisher.Publish(data, x => x.MessageId = msg.Id, ct);
                    return;
                }
                break;
            }
            default:
                // try audit as a generic
                var audit = System.Text.Json.JsonSerializer.Deserialize<AuditLogged>(msg.Payload);
                if (audit != null)
                {
                    await publisher.Publish(audit, x => x.MessageId = msg.Id, ct);
                    return;
                }
                break;
        }

        // if unknown, publish as object to keep bus alive (optional no-op)
    }
}
