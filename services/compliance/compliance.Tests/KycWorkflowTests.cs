using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OIS.Compliance;
using OIS.Compliance.DTOs;
using OIS.Compliance.Services;
using Xunit;

namespace OIS.Compliance.Tests;

public class KycWorkflowTests
{
    private readonly ComplianceDbContext _db;
    private readonly ComplianceService _service;

    public KycWorkflowTests()
    {
        var options = new DbContextOptionsBuilder<ComplianceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new ComplianceDbContext(options);
        var logger = new Mock<ILogger<ComplianceService>>();
        var watchlists = new Mock<IWatchlistsService>();
        var policy = new Mock<IQualificationPolicyService>();
        var outbox = new OutboxService(_db);
        _service = new ComplianceService(_db, logger.Object, watchlists.Object, policy.Object, outbox);
    }

    [Fact]
    public async Task UpdateKycStatus_Changes_Status_And_Writes_Outbox()
    {
        var investorId = Guid.NewGuid();
        var res = await _service.UpdateKycStatusAsync(investorId, "pass", Guid.NewGuid(), "manual approve", CancellationToken.None);
        res.Status.Should().Be("pass");
        var outbox = await _db.OutboxMessages.ToListAsync();
        outbox.Should().NotBeEmpty();
        outbox.Any(m => m.Topic == "ois.kyc.updated").Should().BeTrue();
        outbox.Any(m => m.Topic == "ois.audit.logged").Should().BeTrue();
    }

    [Fact]
    public async Task KycTasks_Create_Approve_Updates_Investor()
    {
        var investorId = Guid.NewGuid();
        var task = await _service.CreateKycTaskAsync(investorId, "doc review", CancellationToken.None);
        task.Status.Should().Be("open");

        var resolved = await _service.ResolveKycTaskAsync(task.Id, "approve", Guid.NewGuid(), null, CancellationToken.None);
        resolved!.Status.Should().Be("approved");
        var status = await _service.GetInvestorStatusAsync(investorId, CancellationToken.None);
        status!.Kyc.Should().Be("pass");
    }
}

