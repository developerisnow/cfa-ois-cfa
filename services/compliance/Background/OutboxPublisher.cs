using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OIS.Contracts.Events;
using Polly;

namespace OIS.Compliance.Background;

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
                var db = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
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
                _logger.LogError(ex, "OutboxPublisher (Compliance) failed");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task PublishTypedAsync(IPublishEndpoint publisher, OutboxMessage msg, CancellationToken ct)
    {
        switch (msg.Topic)
        {
            case "ois.compliance.flagged":
                if (System.Text.Json.JsonSerializer.Deserialize<ComplianceFlagged>(msg.Payload) is { } cf)
                { await publisher.Publish(cf, x => x.MessageId = msg.Id, ct); return; }
                break;
            case "ois.kyc.updated":
                if (System.Text.Json.JsonSerializer.Deserialize<KycUpdated>(msg.Payload) is { } ku)
                { await publisher.Publish(ku, x => x.MessageId = msg.Id, ct); return; }
                break;
            case "ois.audit.logged":
                if (System.Text.Json.JsonSerializer.Deserialize<AuditLogged>(msg.Payload) is { } al)
                { await publisher.Publish(al, x => x.MessageId = msg.Id, ct); return; }
                break;
        }
    }
}

