using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OIS.Domain;
using OIS.Issuance.DTOs;
using OIS.Issuance.Services;
using Xunit;

namespace OIS.Issuance.Tests;

public class IssuanceServiceTests
{
    private readonly IssuanceDbContext _db;
    private readonly Mock<ILogger<IssuanceService>> _logger;
    private readonly Mock<IOutboxService> _outbox;
    private readonly IssuanceService _service;
    private readonly Mock<ILedgerIssuance> _ledger;

    public IssuanceServiceTests()
    {
        var options = new DbContextOptionsBuilder<IssuanceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new IssuanceDbContext(options);
        _logger = new Mock<ILogger<IssuanceService>>();
        _outbox = new Mock<IOutboxService>();
        _ledger = new Mock<ILedgerIssuance>();
        _ledger.Setup(l => l.IssueAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<DateOnly>(), It.IsAny<DateOnly>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("txhash-issue");
        _ledger.Setup(l => l.CloseAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
               .ReturnsAsync("txhash-close");
        _service = new IssuanceService(_db, _logger.Object, _outbox.Object, _ledger.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateDraftIssuance()
    {
        // Arrange
        var request = new CreateIssuanceRequest
        {
            AssetId = Guid.NewGuid(),
            IssuerId = Guid.NewGuid(),
            TotalAmount = 1000000m,
            Nominal = 1000m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
        };

        // Act
        var result = await _service.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("draft");
        result.AssetId.Should().Be(request.AssetId);
        result.IssuerId.Should().Be(request.IssuerId);
    }

    [Fact]
    public async Task PublishAsync_FromDraft_ShouldSucceed()
    {
        // Arrange
        var issuance = new IssuanceEntity
        {
            Id = Guid.NewGuid(),
            AssetId = Guid.NewGuid(),
            IssuerId = Guid.NewGuid(),
            TotalAmount = 1000000m,
            Nominal = 1000m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            Status = IssuanceStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Issuances.Add(issuance);
        await _db.SaveChangesAsync();

        // Act
        var result = await _service.PublishAsync(issuance.Id, CancellationToken.None);

        // Assert
        result.Status.Should().Be("published");
        result.PublishedAt.Should().NotBeNull();
        _outbox.Verify(x => x.AddAsync("ois.issuance.published", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_FromNonDraft_ShouldThrow()
    {
        // Arrange
        var issuance = new IssuanceEntity
        {
            Id = Guid.NewGuid(),
            AssetId = Guid.NewGuid(),
            IssuerId = Guid.NewGuid(),
            TotalAmount = 1000000m,
            Nominal = 1000m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            Status = IssuanceStatus.Published,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Issuances.Add(issuance);
        await _db.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.PublishAsync(issuance.Id, CancellationToken.None));
    }

    [Fact]
    public async Task CloseAsync_FromPublished_ShouldSucceed()
    {
        // Arrange
        var issuance = new IssuanceEntity
        {
            Id = Guid.NewGuid(),
            AssetId = Guid.NewGuid(),
            IssuerId = Guid.NewGuid(),
            TotalAmount = 1000000m,
            Nominal = 1000m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            Status = IssuanceStatus.Published,
            PublishedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Issuances.Add(issuance);
        await _db.SaveChangesAsync();

        // Act
        var result = await _service.CloseAsync(issuance.Id, CancellationToken.None);

        // Assert
        result.Status.Should().Be("closed");
        result.ClosedAt.Should().NotBeNull();
        _outbox.Verify(x => x.AddAsync("ois.issuance.closed", It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CloseAsync_FromNonPublished_ShouldThrow()
    {
        // Arrange
        var issuance = new IssuanceEntity
        {
            Id = Guid.NewGuid(),
            AssetId = Guid.NewGuid(),
            IssuerId = Guid.NewGuid(),
            TotalAmount = 1000000m,
            Nominal = 1000m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
            Status = IssuanceStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _db.Issuances.Add(issuance);
        await _db.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CloseAsync(issuance.Id, CancellationToken.None));
    }
}

