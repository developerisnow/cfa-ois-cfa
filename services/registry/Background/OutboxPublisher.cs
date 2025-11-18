using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OIS.Contracts.Events;
using Polly;

namespace OIS.Registry.Background;

public class OutboxPublisher : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<OutboxPublisher> _logger;

    public OutboxPublisher(IServiceProvider services, ILogger<OutboxPublisher> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RegistryDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var messages = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var msg in messages)
                {
                    var retry = Policy
                        .Handle<Exception>()
                        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

                    await retry.ExecuteAsync(async () =>
                    {
                        await PublishTypedAsync(publisher, msg, stoppingToken);
                    });

                    msg.ProcessedAt = DateTime.UtcNow;
                }

                await db.SaveChangesAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxPublisher (Registry) failed");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task PublishTypedAsync(IPublishEndpoint publisher, OutboxMessage msg, CancellationToken ct)
    {
        switch (msg.Topic)
        {
            case "ois.order.created":
                if (System.Text.Json.JsonSerializer.Deserialize<OrderCreated>(msg.Payload) is { } oc)
                { await publisher.Publish(oc, x => x.MessageId = msg.Id, ct); return; }
                break;
            case "ois.order.placed":
                if (System.Text.Json.JsonSerializer.Deserialize<OrderPlaced>(msg.Payload) is { } placed)
                { await publisher.Publish(placed, x => x.MessageId = msg.Id, ct); return; }
                break;
            case "ois.order.reserved":
                if (System.Text.Json.JsonSerializer.Deserialize<OrderReserved>(msg.Payload) is { } or)
                { await publisher.Publish(or, x => x.MessageId = msg.Id, ct); return; }
                break;
            case "ois.order.paid":
                if (System.Text.Json.JsonSerializer.Deserialize<OrderPaid>(msg.Payload) is { } paid)
                { await publisher.Publish(paid, x => x.MessageId = msg.Id, ct); return; }
                break;
            case "ois.order.confirmed":
                if (System.Text.Json.JsonSerializer.Deserialize<OrderConfirmed>(msg.Payload) is { } ocf)
                { await publisher.Publish(ocf, x => x.MessageId = msg.Id, ct); return; }
                break;
            case "ois.registry.transferred":
                if (System.Text.Json.JsonSerializer.Deserialize<RegistryTransferred>(msg.Payload) is { } rt)
                { await publisher.Publish(rt, x => x.MessageId = msg.Id, ct); return; }
                break;
        }

        // Fallback to audit
        if (System.Text.Json.JsonSerializer.Deserialize<AuditLogged>(msg.Payload) is { } audit)
            await publisher.Publish(audit, x => x.MessageId = msg.Id, ct);
    }
}
