using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using OIS.Compliance.Services;
using Xunit;
using FluentAssertions;

namespace OIS.Compliance.Tests;

public class QualificationPolicyTests
{
    private readonly Mock<ILogger<QualificationPolicyService>> _logger;
    private readonly IConfiguration _config;
    private readonly QualificationPolicyService _service;

    public QualificationPolicyTests()
    {
        _logger = new Mock<ILogger<QualificationPolicyService>>();
        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["QualificationPolicy:Tier1:MaxAmount"] = "100000",
                ["QualificationPolicy:Tier2:MaxAmount"] = "500000",
            })
            .Build();
        _service = new QualificationPolicyService(_logger.Object, _config);
    }

    [Fact]
    public async Task EvaluateQualification_WithAmountWithinLimit_ReturnsAllowed()
    {
        // Arrange
        var investorId = Guid.NewGuid();
        var amount = 50000m;

        // Act
        var result = await _service.EvaluateQualificationAsync(investorId, amount, CancellationToken.None);

        // Assert
        result.Allowed.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateQualification_WithAmountExceedingLimit_ReturnsNotAllowed()
    {
        // Arrange
        var investorId = Guid.NewGuid();
        var amount = 200000m;

        // Act
        var result = await _service.EvaluateQualificationAsync(investorId, amount, CancellationToken.None);

        // Assert
        result.Allowed.Should().BeFalse();
    }
}

