using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OIS.Settlement;
using OIS.Settlement.Services;
using Xunit;
using FluentAssertions;

namespace OIS.Settlement.Tests;

public class IdempotencyTests
{
    private readonly SettlementDbContext _db;
    private readonly Mock<ILogger<SettlementService>> _logger;
    private readonly Mock<IRegistryClient> _registry;
    private readonly Mock<IIssuanceClient> _issuance;
    private readonly Mock<IBankNominalClient> _bank;
    private readonly Mock<IOutboxService> _outbox;
    private readonly SettlementService _service;

    public IdempotencyTests()
    {
        var options = new DbContextOptionsBuilder<SettlementDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new SettlementDbContext(options);
        _logger = new Mock<ILogger<SettlementService>>();
        _registry = new Mock<IRegistryClient>();
        _issuance = new Mock<IIssuanceClient>();
        _bank = new Mock<IBankNominalClient>();
        _outbox = new Mock<IOutboxService>();

        _service = new SettlementService(
            _db,
            _logger.Object,
            _registry.Object,
            _issuance.Object,
            _bank.Object,
            _outbox.Object);
    }

    [Fact]
    public async Task RunSettlement_WithDuplicateDate_ReturnsExistingBatch()
    {
        // Arrange
        var runDate = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var idemKey = $"settlement-{runDate:yyyy-MM-dd}";
        var existingBatch = new PayoutBatchEntity
        {
            Id = Guid.NewGuid(),
            RunDate = runDate,
            Status = "completed",
            IdemKey = idemKey,
            CreatedAt = DateTime.UtcNow,
        };
        _db.PayoutBatches.Add(existingBatch);
        await _db.SaveChangesAsync();

        // Act
        var result = await _service.RunSettlementAsync(runDate, CancellationToken.None);

        // Assert
        result.BatchId.Should().Be(existingBatch.Id);
        
        // Verify no new batch was created
        var batchCount = await _db.PayoutBatches.CountAsync();
        batchCount.Should().Be(1);
    }
}

