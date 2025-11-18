using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OIS.Registry;
using OIS.Registry.DTOs;
using OIS.Registry.Services;
using Xunit;

namespace OIS.Registry.Tests;

public class OrderFlowTests
{
    private readonly RegistryDbContext _db;
    private readonly Mock<IComplianceService> _compliance;
    private readonly Mock<IBankNominalService> _bank;
    private readonly Mock<ILedgerRegistry> _ledger;
    private readonly IRegistryService _service;

    public OrderFlowTests()
    {
        var options = new DbContextOptionsBuilder<RegistryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new RegistryDbContext(options);
        _compliance = new Mock<IComplianceService>();
        _bank = new Mock<IBankNominalService>();
        _ledger = new Mock<ILedgerRegistry>();
        _compliance.Setup(x => x.CheckKycAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _compliance.Setup(x => x.CheckQualificationAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _bank.Setup(x => x.ReserveFundsAsync(It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync("bank-transfer-1");
        _ledger.Setup(x => x.TransferAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>())).ReturnsAsync("txhash-abc");

        var logger = new Mock<ILogger<RegistryService>>();
        var outbox = new OutboxService(_db);
        _service = new RegistryService(_db, logger.Object, _compliance.Object, _bank.Object, _ledger.Object, outbox);
    }

    [Fact]
    public async Task PlaceOrder_IsIdempotent_And_Reserved()
    {
        var investor = Guid.NewGuid();
        var issuance = Guid.NewGuid();
        var req = new CreateOrderRequest { InvestorId = investor, IssuanceId = issuance, Amount = 100m };
        var idem = Guid.NewGuid().ToString();

        var r1 = await _service.PlaceOrderAsync(req, idem, CancellationToken.None);
        var r2 = await _service.PlaceOrderAsync(req, idem, CancellationToken.None);

        r1.Id.Should().Be(r2.Id);
        r1.Status.Should().Be("reserved");

        var topics = await _db.OutboxMessages
            .OrderBy(m => m.CreatedAt)
            .Select(m => m.Topic)
            .ToListAsync();

        topics.Should().Contain("ois.order.created");
        topics.Should().Contain("ois.order.placed");
        topics.Should().Contain("ois.order.reserved");
    }

    [Fact]
    public async Task MarkPaid_Moves_To_Paid_And_Writes_Tx()
    {
        var investor = Guid.NewGuid();
        var issuance = Guid.NewGuid();
        var req = new CreateOrderRequest { InvestorId = investor, IssuanceId = issuance, Amount = 55m };
        var idem = Guid.NewGuid().ToString();

        var order = await _service.PlaceOrderAsync(req, idem, CancellationToken.None);
        var paid = await _service.MarkPaidAsync(order.Id, null, CancellationToken.None);

        paid.Status.Should().Be("paid");
        paid.DltTxHash.Should().NotBeNull();
        var tx = await _db.Transactions.FirstOrDefaultAsync(t => t.Id == order.Id);
        tx.Should().NotBeNull();
        tx!.Status.Should().Be("confirmed");

        var topics = await _db.OutboxMessages
            .Where(m => m.Topic.StartsWith("ois.order.") || m.Topic == "ois.registry.transferred")
            .Select(m => m.Topic)
            .ToListAsync();

        topics.Should().Contain("ois.order.paid");
        topics.Should().Contain("ois.registry.transferred");
        topics.Should().Contain("ois.order.confirmed");
    }

    [Fact]
    public async Task MarkPaid_On_Ledger_Error_Stays_Reserved_And_Allows_Retry()
    {
        var investor = Guid.NewGuid();
        var issuance = Guid.NewGuid();
        var req = new CreateOrderRequest { InvestorId = investor, IssuanceId = issuance, Amount = 10m };
        var idem = Guid.NewGuid().ToString();

        var order = await _service.PlaceOrderAsync(req, idem, CancellationToken.None);

        // now make ledger fail
        _ledger.Setup(x => x.TransferAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
               .ThrowsAsync(new InvalidOperationException("dlterr"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MarkPaidAsync(order.Id, null, CancellationToken.None));

        var entity = await _db.Orders.FindAsync(order.Id);
        entity!.Status.Should().Be("reserved");

        // restore ledger and retry -> should succeed
        _ledger.Setup(x => x.TransferAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("txhash-retry");

        var paid = await _service.MarkPaidAsync(order.Id, null, CancellationToken.None);
        paid.Status.Should().Be("paid");
        paid.DltTxHash.Should().Be("txhash-retry");
    }
}
