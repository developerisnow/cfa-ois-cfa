using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OIS.Contracts.Events;
using Polly;

namespace OIS.Issuance.Background;

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
                var db = scope.ServiceProvider.GetRequiredService<IssuanceDbContext>();
                var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

                var messages = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .OrderBy(x => x.CreatedAt)
                    .Take(50)
                    .ToListAsync(stoppingToken);

                foreach (var msg in messages)
                {
                    var retry = Polly.Policy
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
                _logger.LogError(ex, "OutboxPublisher (Issuance) failed");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task PublishTypedAsync(IPublishEndpoint publisher, OutboxMessage msg, CancellationToken ct)
    {
        switch (msg.Topic)
        {
            case "ois.issuance.published":
                if (System.Text.Json.JsonSerializer.Deserialize<IssuancePublished>(msg.Payload) is { } ip)
                { await publisher.Publish(ip, x => x.MessageId = msg.Id, ct); return; }
                break;
            case "ois.issuance.closed":
                if (System.Text.Json.JsonSerializer.Deserialize<IssuanceClosed>(msg.Payload) is { } ic)
                { await publisher.Publish(ic, x => x.MessageId = msg.Id, ct); return; }
                break;
        }

        if (System.Text.Json.JsonSerializer.Deserialize<AuditLogged>(msg.Payload) is { } audit)
            await publisher.Publish(audit, x => x.MessageId = msg.Id, ct);
    }
}

