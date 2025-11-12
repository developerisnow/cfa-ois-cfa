using MassTransit;
using Microsoft.EntityFrameworkCore;
using OIS.Contracts.Events;

namespace OIS.Settlement.Consumers;

public class OrderPaidEventConsumer : IConsumer<OrderPaid>
{
    private readonly SettlementDbContext _db;
    private readonly ILogger<OrderPaidEventConsumer> _logger;

    public OrderPaidEventConsumer(SettlementDbContext db, ILogger<OrderPaidEventConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderPaid> context)
    {
        var messageId = context.MessageId?.ToString();
        var consumerName = nameof(OrderPaidEventConsumer);
        if (string.IsNullOrEmpty(messageId))
        {
            _logger.LogWarning("OrderPaid message has no MessageId; skipping idempotency");
        }
        else
        {
            var exists = await _db.ProcessedMessages.AnyAsync(x => x.MessageId == messageId && x.Consumer == consumerName, context.CancellationToken);
            if (exists)
            {
                _logger.LogInformation("Duplicate OrderPaid message {MessageId} ignored", messageId);
                return;
            }
        }

        // minimal: just log, business postings handled in services as needed
        _logger.LogInformation("OrderPaid consumed: orderId={OrderId}, investor={InvestorId}, issuance={IssuanceId}, amount={Amount}",
            context.Message.orderId, context.Message.investorId, context.Message.issuanceId, context.Message.amount);

        if (!string.IsNullOrEmpty(messageId))
        {
            _db.ProcessedMessages.Add(new ProcessedMessage
            {
                MessageId = messageId!,
                Consumer = consumerName,
                ProcessedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(context.CancellationToken);
        }
    }
}

