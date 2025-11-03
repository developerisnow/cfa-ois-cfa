using FluentAssertions;
using OIS.Domain;
using Xunit;

namespace OIS.Domain.Tests;

public class ScheduleItemTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldSucceed()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var amount = 1000m;

        // Act
        var item = new ScheduleItem(date, amount, "Test payment");

        // Assert
        item.Date.Should().Be(date);
        item.Amount.Should().Be(amount);
        item.Description.Should().Be("Test payment");
    }

    [Fact]
    public void Constructor_WithZeroAmount_ShouldThrow()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ScheduleItem(date, 0m));
    }

    [Fact]
    public void Constructor_WithNegativeAmount_ShouldThrow()
    {
        // Arrange
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => new ScheduleItem(date, -100m));
    }
}

public class PayoutScheduleTests
{
    [Fact]
    public void Constructor_WithItems_ShouldSucceed()
    {
        // Arrange
        var items = new[]
        {
            new ScheduleItem(DateOnly.FromDateTime(DateTime.UtcNow), 1000m),
            new ScheduleItem(DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 500m)
        };

        // Act
        var schedule = new PayoutSchedule(items);

        // Assert
        schedule.Items.Should().HaveCount(2);
        schedule.TotalAmount.Should().Be(1500m);
    }

    [Fact]
    public void Constructor_WithEmptyItems_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new PayoutSchedule(Array.Empty<ScheduleItem>()));
    }

    [Fact]
    public void Constructor_WithNullItems_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new PayoutSchedule(null!));
    }
}

