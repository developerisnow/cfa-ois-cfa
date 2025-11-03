using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OIS.Registry;
using OIS.Registry.DTOs;
using OIS.Registry.Services;
using Xunit;
using FluentAssertions;

namespace OIS.Registry.Tests;

public class IdempotencyTests
{
    private readonly RegistryDbContext _db;
    private readonly Mock<ILogger<RegistryService>> _logger;
    private readonly Mock<IComplianceService> _compliance;
    private readonly Mock<IBankNominalService> _bank;
    private readonly Mock<ILedgerRegistry> _ledger;
    private readonly Mock<IOutboxService> _outbox;
    private readonly RegistryService _service;

    public IdempotencyTests()
    {
        var options = new DbContextOptionsBuilder<RegistryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new RegistryDbContext(options);
        _logger = new Mock<ILogger<RegistryService>>();
        _compliance = new Mock<IComplianceService>();
        _bank = new Mock<IBankNominalService>();
        _ledger = new Mock<ILedgerRegistry>();
        _outbox = new Mock<IOutboxService>();

        _service = new RegistryService(
            _db,
            _logger.Object,
            _compliance.Object,
            _bank.Object,
            _ledger.Object,
            _outbox.Object);
    }

    [Fact]
    public async Task PlaceOrder_WithDuplicateIdempotencyKey_ReturnsExistingOrder()
    {
        // Arrange
        var idemKey = "test-idem-key-123";
        var existingOrder = new OrderEntity
        {
            Id = Guid.NewGuid(),
            InvestorId = Guid.NewGuid(),
            IssuanceId = Guid.NewGuid(),
            Amount = 1000,
            Status = "pending",
            IdemKey = idemKey,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Orders.Add(existingOrder);
        await _db.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            InvestorId = existingOrder.InvestorId,
            IssuanceId = existingOrder.IssuanceId,
            Amount = 1000,
        };

        // Act
        var result = await _service.PlaceOrderAsync(request, idemKey, CancellationToken.None);

        // Assert
        Assert.Equal(existingOrder.Id, result.Id);
        Assert.Equal(existingOrder.Status, result.Status);
        
        // Verify no new order was created
        var orderCount = await _db.Orders.CountAsync();
        Assert.Equal(1, orderCount);
        
        // Verify compliance/bank were not called (idempotent return)
        _compliance.Verify(x => x.CheckKycAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _bank.Verify(x => x.ReserveFundsAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PlaceOrder_WithNewIdempotencyKey_CreatesNewOrder()
    {
        // Arrange
        var idemKey = "new-idem-key-456";
        _compliance.Setup(x => x.CheckKycAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _compliance.Setup(x => x.CheckQualificationAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _bank.Setup(x => x.ReserveFundsAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("transfer-123");

        var request = new CreateOrderRequest
        {
            InvestorId = Guid.NewGuid(),
            IssuanceId = Guid.NewGuid(),
            Amount = 5000,
        };

        // Act
        var result = await _service.PlaceOrderAsync(request, idemKey, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Amount, result.Amount);
        
        var orderCount = await _db.Orders.CountAsync();
        Assert.Equal(1, orderCount);
        
        _compliance.Verify(x => x.CheckKycAsync(request.InvestorId, It.IsAny<CancellationToken>()), Times.Once);
    }
}

