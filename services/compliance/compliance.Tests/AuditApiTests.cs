using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OIS.Compliance;
using Xunit;

namespace OIS.Compliance.Tests;

public class AuditApiTests : IClassFixture<ComplianceFactory>
{
    private readonly ComplianceFactory _factory;
    public AuditApiTests(ComplianceFactory factory) { _factory = factory; }

    [Fact]
    public async Task GetAudit_Returns_Items_In_Order()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");

        // seed one audit outbox message
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ComplianceDbContext>();
            db.OutboxMessages.Add(new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Topic = "ois.audit.logged",
                Payload = JsonSerializer.Serialize(new
                {
                    id = Guid.NewGuid(),
                    actor = Guid.NewGuid(),
                    action = "kyc.update",
                    entity = "investor",
                    timestamp = DateTime.UtcNow,
                }),
                CreatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var res = await client.GetAsync("/v1/audit");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await res.Content.ReadFromJsonAsync<JsonElement>();
        body.TryGetProperty("items", out var items).Should().BeTrue();
        items.GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ExportCsv_Returns_Csv_Content()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test");
        var res = await client.GetAsync("/v1/audit/export.csv");
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        res.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var text = await res.Content.ReadAsStringAsync();
        text.Should().Contain("id,actor,actorName,action,entity,entityId,result,timestamp,ip,userAgent");
    }
}

public class ComplianceFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace DB with InMemory
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ComplianceDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<ComplianceDbContext>(o => o.UseInMemoryDatabase("compliance-audit-tests"));

            // Bypass auth; always authenticate with backoffice role
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(JwtBearerDefaults.AuthenticationScheme, options => { });
        });
    }
}

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, System.Text.Encodings.Web.UrlEncoder encoder, ISystemClock clock)
        : base(options, logger, encoder, clock) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, "backoffice")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

