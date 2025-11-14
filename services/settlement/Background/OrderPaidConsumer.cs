using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OIS.Settlement.Infrastructure;
using Polly;

namespace OIS.Settlement.Background;

public class OrderPaidConsumer : BackgroundService
{
    private readonly ILogger<OrderPaidConsumer> _logger;
    private readonly IServiceProvider _services;
    private readonly IConfiguration _configuration;

    public OrderPaidConsumer(ILogger<OrderPaidConsumer> logger, IServiceProvider services, IConfiguration configuration)
    {
        _logger = logger;
        _services = services;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var enabled = _configuration.GetValue<bool>("Kafka:Enabled", true);
        if (!enabled)
        {
            _logger.LogWarning("Kafka disabled, OrderPaidConsumer will not start");
            return;
        }

        var bootstrap = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";
        var groupId = _configuration["Kafka:GroupId:OrderPaid"] ?? "settlement-orderpaid-consumer";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrap,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe("ois.order.paid");

        _logger.LogInformation("OrderPaidConsumer started, bootstrap={BootstrapServers}, group={GroupId}", bootstrap, groupId);

        var retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                (ex, ts, attempt, ctx) => _logger.LogWarning(ex, "Retry {Attempt} after {Delay}ms", attempt, ts.TotalMilliseconds));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? cr = null;
                try
                {
                    cr = consumer.Consume(stoppingToken);
                    if (cr == null) continue;

                    await retry.ExecuteAsync(async () =>
                    {
                        await HandleMessageAsync(cr.Message.Key, cr.Message.Value, stoppingToken);
                    });

                    Metrics.EventsProcessed.Add(1, new("event", "order.paid"));
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process message at {TopicPartitionOffset}", cr?.TopicPartitionOffset);
                    Metrics.EventsFailed.Add(1, new("event", "order.paid"));
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task HandleMessageAsync(string? key, string payload, CancellationToken ct)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;
            var orderId = root.GetProperty("orderId").GetGuid();
            var investorId = root.GetProperty("investorId").GetGuid();
            var issuanceId = root.GetProperty("issuanceId").GetGuid();
            var amount = root.GetProperty("amount").GetDecimal();
            var paidAt = root.TryGetProperty("paidAt", out var paidAtEl) && paidAtEl.ValueKind == JsonValueKind.String
                ? DateTime.Parse(paidAtEl.GetString()!)
                : DateTime.UtcNow;
            var txHash = root.TryGetProperty("txHash", out var txEl) ? txEl.GetString() : null;

            _logger.LogInformation("Received order.paid: orderId={OrderId}, investor={InvestorId}, issuance={IssuanceId}, amount={Amount}, tx={TxHash}",
                orderId, investorId, issuanceId, amount, txHash);

            // For now we only record a lightweight log and expose metrics.
            // Further accounting postings can be added here when schema is defined.
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing/handling order.paid payload: {Payload}", payload);
            throw;
        }
    }
}

