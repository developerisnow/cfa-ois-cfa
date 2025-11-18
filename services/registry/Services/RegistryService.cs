using Microsoft.EntityFrameworkCore;
using OIS.Registry;
using OIS.Registry.DTOs;
using System.Diagnostics;
using System.Text.Json;

namespace OIS.Registry.Services;

public interface IRegistryService
{
    Task<OrderResponse> PlaceOrderAsync(CreateOrderRequest request, string idempotencyKey, CancellationToken ct);
    Task<OrderResponse?> GetOrderAsync(Guid orderId, CancellationToken ct);
    Task<WalletResponse?> GetWalletAsync(Guid investorId, CancellationToken ct);
    Task<RedeemResponse> RedeemAsync(Guid issuanceId, RedeemRequest request, CancellationToken ct);
    Task<OrderResponse> CancelOrderAsync(Guid orderId, CancellationToken ct);
    Task<OrderResponse> MarkPaidAsync(Guid orderId, string? paymentRef, CancellationToken ct);
}

public class RegistryService : IRegistryService
{
    private readonly RegistryDbContext _db;
    private readonly ILogger<RegistryService> _logger;
    private readonly IComplianceService _compliance;
    private readonly IBankNominalService _bank;
    private readonly ILedgerRegistry _ledger;
    private readonly IOutboxService _outbox;

    public RegistryService(
        RegistryDbContext db,
        ILogger<RegistryService> logger,
        IComplianceService compliance,
        IBankNominalService bank,
        ILedgerRegistry ledger,
        IOutboxService outbox)
    {
        _db = db;
        _logger = logger;
        _compliance = compliance;
        _bank = bank;
        _ledger = ledger;
        _outbox = outbox;
    }

    public async Task<OrderResponse> PlaceOrderAsync(CreateOrderRequest request, string idempotencyKey, CancellationToken ct)
    {
        // Check idempotency
        var existingOrder = await _db.Orders
            .FirstOrDefaultAsync(o => o.IdemKey == idempotencyKey, ct);
        
        if (existingOrder != null)
        {
            _logger.LogInformation("Order with idempotency key {IdemKey} already exists: {OrderId}", 
                idempotencyKey, existingOrder.Id);
            return MapToOrderResponse(existingOrder);
        }

        // (a) Validate KYC/qualification
        var kycOk = await _compliance.CheckKycAsync(request.InvestorId, ct);
        if (!kycOk)
            throw new InvalidOperationException($"KYC check failed for investor {request.InvestorId}");

        var qualOk = await _compliance.CheckQualificationAsync(request.InvestorId, request.Amount, ct);
        if (!qualOk)
            throw new InvalidOperationException($"Qualification check failed for investor {request.InvestorId}: limit exceeded or not qualified");

        // Create order (created)
        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            InvestorId = request.InvestorId,
            IssuanceId = request.IssuanceId,
            Amount = request.Amount,
            Status = "created",
            IdemKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);

        // Emit order.created
        await _outbox.AddAsync("ois.order.created", new
        {
            orderId = order.Id,
            issuanceId = request.IssuanceId,
            investorId = request.InvestorId,
            amount = request.Amount,
            createdAt = order.CreatedAt
        }, ct);

        // Emit order.placed (business-level event: order accepted before funds reservation)
        await _outbox.AddAsync("ois.order.placed", new
        {
            orderId = order.Id,
            issuanceId = request.IssuanceId,
            investorId = request.InvestorId,
            amount = request.Amount,
            placedAt = order.CreatedAt
        }, ct);
        await _db.SaveChangesAsync(ct);

        // (b) Reserve funds via bank-nominal (idempotent)
        string transferId;
        try
        {
            transferId = await _bank.ReserveFundsAsync(request.InvestorId, request.Amount, idempotencyKey, ct);
            _logger.LogInformation("Funds reserved: transferId={TransferId}, investor={Investor}, amount={Amount}",
                transferId, OIS.Domain.Security.MaskGuid(request.InvestorId), request.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve funds for investor {Investor}", OIS.Domain.Security.MaskGuid(request.InvestorId));
            throw new InvalidOperationException($"Failed to reserve funds: {ex.Message}", ex);
        }

        // Update order to reserved
        order.Status = "reserved";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        await _outbox.AddAsync("ois.order.reserved", new
        {
            orderId = order.Id,
            issuanceId = request.IssuanceId,
            investorId = request.InvestorId,
            amount = request.Amount,
            reservedAt = order.UpdatedAt,
            bankTransferId = transferId
        }, ct);

        await _db.SaveChangesAsync(ct);

        return MapToOrderResponse(order);
    }

    public async Task<OrderResponse?> GetOrderAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.FindAsync(new object[] { orderId }, ct);
        return order != null ? MapToOrderResponse(order) : null;
    }

    public async Task<WalletResponse?> GetWalletAsync(Guid investorId, CancellationToken ct)
    {
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.OwnerId == investorId && w.OwnerType == "individual", ct);

        if (wallet == null)
            return null;

        var holdings = await _db.Holdings
            .Where(h => h.InvestorId == investorId)
            .Select(h => new HoldingDto
            {
                IssuanceId = h.IssuanceId,
                Quantity = h.Quantity,
                UpdatedAt = h.UpdatedAt
            })
            .ToListAsync(ct);

        return new WalletResponse
        {
            InvestorId = investorId,
            Balance = wallet.Balance,
            Blocked = wallet.Blocked,
            Holdings = holdings
        };
    }

    public async Task<RedeemResponse> RedeemAsync(Guid issuanceId, RedeemRequest request, CancellationToken ct)
    {
        // TODO: Get investor from context/auth
        var investorId = Guid.NewGuid(); // Placeholder

        var stopwatch = Stopwatch.StartNew();
        string txHash;
        try
        {
            txHash = await _ledger.RedeemAsync(investorId.ToString(), issuanceId, request.Amount, ct);
            stopwatch.Stop();
            _logger.LogInformation(
                "Ledger Redeem successful: issuanceId={IssuanceId}, txHash={TxHash}, duration={Duration}ms",
                issuanceId, txHash, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Ledger Redeem failed for issuance {IssuanceId}", issuanceId);
            throw new InvalidOperationException($"Failed to redeem on ledger: {ex.Message}", ex);
        }

        // Update holding
        var holding = await _db.Holdings
            .FirstOrDefaultAsync(h => h.InvestorId == investorId && h.IssuanceId == issuanceId, ct);
        
        if (holding != null)
        {
            holding.Quantity -= request.Amount;
            holding.UpdatedAt = DateTime.UtcNow;
        }

        await WriteTransactionAsync(Guid.NewGuid(), "redeem", null, null, issuanceId, request.Amount, txHash, ct);
        await _db.SaveChangesAsync(ct);

        return new RedeemResponse
        {
            IssuanceId = issuanceId,
            RedeemedAmount = request.Amount,
            DltTxHash = txHash,
            RedeemedAt = DateTime.UtcNow
        };
    }

    public async Task<OrderResponse> CancelOrderAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.FindAsync(new object[] { orderId }, ct)
            ?? throw new InvalidOperationException($"Order {orderId} not found");

        if (order.Status is "paid" or "cancelled")
            throw new InvalidOperationException($"Cannot cancel order in status {order.Status}");

        order.Status = "cancelled";
        order.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return MapToOrderResponse(order);
    }

    public async Task<OrderResponse> MarkPaidAsync(Guid orderId, string? paymentRef, CancellationToken ct)
    {
        var order = await _db.Orders.FindAsync(new object[] { orderId }, ct)
            ?? throw new InvalidOperationException($"Order {orderId} not found");

        // Idempotent: if already paid, return current state
        if (order.Status == "paid")
            return MapToOrderResponse(order);

        if (order.Status != "reserved")
            throw new InvalidOperationException($"Order must be in 'reserved' status to mark paid; actual: {order.Status}");

        // (c) Call ledger registry.Transfer
        var stopwatch = Stopwatch.StartNew();
        string txHash;
        try
        {
            txHash = await _ledger.TransferAsync(null, order.InvestorId.ToString(), order.IssuanceId, order.Amount, ct);
            stopwatch.Stop();
            _logger.LogInformation(
                "Ledger Transfer successful: orderId={OrderId}, txHash={TxHash}, duration={Duration}ms",
                order.Id, txHash, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex,
                "Ledger Transfer failed for order {OrderId} after {Duration}ms",
                order.Id, stopwatch.ElapsedMilliseconds);
            // Keep order in 'reserved' status to allow retry; record failure reason transiently
            order.FailureReason = $"Ledger error: {ex.Message}";
            await _db.SaveChangesAsync(ct);
            throw;
        }

        // Update order -> paid
        order.Status = "paid";
        order.DltTxHash = txHash;
        order.ConfirmedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        // Wallet + holding
        var wallet = await GetOrCreateWalletAsync(order.InvestorId, "individual", ct);
        order.WalletId = wallet.Id;
        await UpdateHoldingAsync(order.InvestorId, order.IssuanceId, order.Amount, ct);

        // Transactions and events
        await WriteTransactionAsync(order.Id, "transfer", null, wallet.Id, order.IssuanceId, order.Amount, txHash, ct);

        await _outbox.AddAsync("ois.order.paid", new
        {
            orderId = order.Id,
            issuanceId = order.IssuanceId,
            investorId = order.InvestorId,
            amount = order.Amount,
            paidAt = order.ConfirmedAt,
            txHash = txHash
        }, ct);

        await _outbox.AddAsync("ois.order.confirmed", new
        {
            orderId = order.Id,
            confirmedAt = order.ConfirmedAt,
            dltTxHash = txHash,
            walletId = wallet.Id
        }, ct);

        await _outbox.AddAsync("ois.registry.transferred", new
        {
            orderId = order.Id,
            issuanceId = order.IssuanceId,
            investorId = order.InvestorId,
            amount = order.Amount,
            txHash = txHash,
            walletId = wallet.Id,
            transferredAt = order.ConfirmedAt
        }, ct);

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Order {OrderId} marked as paid with txHash {TxHash}", order.Id, txHash);

        return MapToOrderResponse(order);
    }

    private async Task<WalletEntity> GetOrCreateWalletAsync(Guid ownerId, string ownerType, CancellationToken ct)
    {
        var wallet = await _db.Wallets
            .FirstOrDefaultAsync(w => w.OwnerId == ownerId && w.OwnerType == ownerType, ct);

        if (wallet == null)
        {
            wallet = new WalletEntity
            {
                Id = Guid.NewGuid(),
                OwnerId = ownerId,
                OwnerType = ownerType,
                Balance = 0,
                Blocked = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Wallets.Add(wallet);
            await _db.SaveChangesAsync(ct);
        }

        return wallet;
    }

    private async Task UpdateHoldingAsync(Guid investorId, Guid issuanceId, decimal quantity, CancellationToken ct)
    {
        var holding = await _db.Holdings
            .FirstOrDefaultAsync(h => h.InvestorId == investorId && h.IssuanceId == issuanceId, ct);

        if (holding == null)
        {
            holding = new HoldingEntity
            {
                Id = Guid.NewGuid(),
                InvestorId = investorId,
                IssuanceId = issuanceId,
                Quantity = 0,
                UpdatedAt = DateTime.UtcNow
            };
            _db.Holdings.Add(holding);
        }

        holding.Quantity += quantity;
        holding.UpdatedAt = DateTime.UtcNow;
    }

    private async Task WriteTransactionAsync(
        Guid id,
        string type,
        Guid? fromWalletId,
        Guid? toWalletId,
        Guid? issuanceId,
        decimal amount,
        string txHash,
        CancellationToken ct)
    {
        var tx = new TransactionEntity
        {
            Id = id,
            Type = type,
            FromWalletId = fromWalletId,
            ToWalletId = toWalletId,
            IssuanceId = issuanceId,
            Amount = amount,
            DltTxHash = txHash,
            Status = "confirmed",
            CreatedAt = DateTime.UtcNow,
            ConfirmedAt = DateTime.UtcNow
        };

        _db.Transactions.Add(tx);
    }

    private static OrderResponse MapToOrderResponse(OrderEntity entity)
    {
        return new OrderResponse
        {
            Id = entity.Id,
            InvestorId = entity.InvestorId,
            IssuanceId = entity.IssuanceId,
            Amount = entity.Amount,
            Status = entity.Status,
            WalletId = entity.WalletId,
            DltTxHash = entity.DltTxHash,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ConfirmedAt = entity.ConfirmedAt,
            FailureReason = entity.FailureReason
        };
    }
}

public interface IOutboxService
{
    Task AddAsync(string topic, object payload, CancellationToken ct);
}

public class OutboxService : IOutboxService
{
    private readonly RegistryDbContext _db;

    public OutboxService(RegistryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(string topic, object payload, CancellationToken ct)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        _db.OutboxMessages.Add(message);
    }
}
