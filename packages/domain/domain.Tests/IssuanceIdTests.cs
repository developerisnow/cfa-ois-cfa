using FluentAssertions;
using OIS.Domain;
using Xunit;

namespace OIS.Domain.Tests;

public class IssuanceIdTests
{
    [Fact]
    public void Create_ShouldGenerateNewGuid()
    {
        // Act
        var id1 = IssuanceId.Create();
        var id2 = IssuanceId.Create();

        // Assert
        id1.Value.Should().NotBe(Guid.Empty);
        id2.Value.Should().NotBe(Guid.Empty);
        id1.Value.Should().NotBe(id2.Value);
    }

    [Fact]
    public void From_WithValidGuid_ShouldSucceed()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var id = IssuanceId.From(guid);

        // Assert
        id.Value.Should().Be(guid);
    }

    [Fact]
    public void From_WithEmptyGuid_ShouldThrow()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => IssuanceId.From(Guid.Empty));
    }

    [Fact]
    public void ImplicitConversion_ShouldWork()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var id = IssuanceId.From(guid);

        // Act
        Guid converted = id;
        IssuanceId convertedBack = guid;

        // Assert
        converted.Should().Be(guid);
        convertedBack.Value.Should().Be(guid);
    }
}

