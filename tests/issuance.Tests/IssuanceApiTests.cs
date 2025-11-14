using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OIS.Issuance;
using OIS.Issuance.DTOs;
using Xunit;

namespace OIS.Issuance.Tests;

public class IssuanceApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public IssuanceApiTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        return client;
    }

    [Fact]
    public async Task Publish_NonExistent_Should_Return_404()
    {
        var client = CreateClient();
        var res = await client.PostAsync($"/v1/issuances/{Guid.NewGuid()}/publish", content: null);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Kafka:Enabled", "false");
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Kafka:Enabled"] = "false",
                ["RunMigrations"] = "false",
                ["DisableHttpsRedirection"] = "true",
                ["Ledger:UseMock"] = "true",
                ["Ledger:ChaincodeEndpoint"] = ""
            });
        });

        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<IssuanceDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<IssuanceDbContext>(options => options.UseInMemoryDatabase("IssuanceTestsDb"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
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
            new Claim(ClaimTypes.Role, "issuer")
        };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

