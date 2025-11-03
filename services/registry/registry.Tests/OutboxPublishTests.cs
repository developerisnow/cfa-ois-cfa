using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OIS.Registry;
using OIS.Registry.Services;
using System.Text.Json;
using Xunit;

namespace OIS.Registry.Tests;

public class OutboxPublishTests
{
    private readonly RegistryDbContext _db;
    private readonly OutboxService _outboxService;

    public OutboxPublishTests()
    {
        var options = new DbContextOptionsBuilder<RegistryDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _db = new RegistryDbContext(options);
        _outboxService = new OutboxService(_db);
    }

    [Fact]
    public async Task AddAsync_CreatesOutboxMessage()
    {
        // Arrange
        var eventPayload = new
        {
            orderId = Guid.NewGuid().ToString(),
            investorId = Guid.NewGuid().ToString(),
            amount = 5000,
        };

        // Act
        await _outboxService.AddAsync("ois.order.placed", eventPayload, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        var messages = await _db.OutboxMessages.ToListAsync();
        Assert.Single(messages);
        Assert.Equal("ois.order.placed", messages[0].Topic);
        Assert.Contains("orderId", messages[0].Payload);
    }

    [Fact]
    public async Task AddAsync_WithMultipleEvents_CreatesAllMessages()
    {
        // Arrange & Act
        await _outboxService.AddAsync("topic1", new { data = "1" }, CancellationToken.None);
        await _outboxService.AddAsync("topic2", new { data = "2" }, CancellationToken.None);
        await _outboxService.AddAsync("topic3", new { data = "3" }, CancellationToken.None);
        await _db.SaveChangesAsync();

        // Assert
        var messages = await _db.OutboxMessages.ToListAsync();
        Assert.Equal(3, messages.Count);
    }
}

