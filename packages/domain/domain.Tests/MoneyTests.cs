using FluentAssertions;
using OIS.Domain;
using Xunit;

namespace OIS.Domain.Tests;

public class MoneyTests
{
    [Fact]
    public void Create_WithValidAmount_ShouldSucceed()
    {
        // Act
        var money = Money.Create(100.50m, "RUB");

        // Assert
        money.Amount.Should().Be(100.50m);
        money.Currency.Should().Be("RUB");
    }

    [Fact]
    public void Create_WithNegativeAmount_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Money.Create(-1m));
    }

    [Fact]
    public void Create_WithEmptyCurrency_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Money.Create(100m, ""));
    }

    [Fact]
    public void Add_SameCurrency_ShouldSucceed()
    {
        // Arrange
        var left = Money.Create(100m);
        var right = Money.Create(50m);

        // Act
        var result = left + right;

        // Assert
        result.Amount.Should().Be(150m);
    }

    [Fact]
    public void Add_DifferentCurrencies_ShouldThrow()
    {
        // Arrange
        var left = Money.Create(100m, "RUB");
        var right = Money.Create(50m, "USD");

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => left + right);
    }

    [Fact]
    public void Subtract_SameCurrency_ShouldSucceed()
    {
        // Arrange
        var left = Money.Create(100m);
        var right = Money.Create(30m);

        // Act
        var result = left - right;

        // Assert
        result.Amount.Should().Be(70m);
    }

    [Fact]
    public void Subtract_ResultNegative_ShouldThrow()
    {
        // Arrange
        var left = Money.Create(50m);
        var right = Money.Create(100m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => left - right);
    }
}

