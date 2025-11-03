using FluentAssertions;
using OIS.Domain;
using Xunit;

namespace OIS.Domain.Tests;

public class IssuanceStatusTests
{
    [Theory]
    [InlineData(IssuanceStatus.Draft, IssuanceStatus.Published, true)]
    [InlineData(IssuanceStatus.Published, IssuanceStatus.Closed, true)]
    [InlineData(IssuanceStatus.Closed, IssuanceStatus.Redeemed, true)]
    [InlineData(IssuanceStatus.Draft, IssuanceStatus.Closed, false)]
    [InlineData(IssuanceStatus.Published, IssuanceStatus.Redeemed, false)]
    [InlineData(IssuanceStatus.Closed, IssuanceStatus.Published, false)]
    public void CanTransitionTo_ShouldReturnExpected(
        IssuanceStatus from, 
        IssuanceStatus to, 
        bool expected)
    {
        // Act
        var result = from.CanTransitionTo(to);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(IssuanceStatus.Draft, "draft")]
    [InlineData(IssuanceStatus.Published, "published")]
    [InlineData(IssuanceStatus.Closed, "closed")]
    [InlineData(IssuanceStatus.Redeemed, "redeemed")]
    public void ToStringValue_ShouldReturnLowerCase(IssuanceStatus status, string expected)
    {
        // Act
        var result = status.ToStringValue();

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("draft", IssuanceStatus.Draft)]
    [InlineData("published", IssuanceStatus.Published)]
    [InlineData("closed", IssuanceStatus.Closed)]
    [InlineData("redeemed", IssuanceStatus.Redeemed)]
    public void FromString_ShouldParseCorrectly(string value, IssuanceStatus expected)
    {
        // Act
        var result = IssuanceStatusExtensions.FromString(value);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void FromString_InvalidValue_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => IssuanceStatusExtensions.FromString("invalid"));
    }
}

