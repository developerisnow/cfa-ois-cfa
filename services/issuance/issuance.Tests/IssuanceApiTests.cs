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
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");
        return client;
    }

    [Fact]
    public async Task Create_Then_Get_Should_Return_Issuance()
    {
        var client = CreateClient();
        var req = new CreateIssuanceRequest
        {
            AssetId = Guid.NewGuid(),
            IssuerId = Guid.NewGuid(),
            TotalAmount = 1000m,
            Nominal = 10m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30).Date),
            ScheduleJson = new Dictionary<string, object> { ["items"] = new[] { new { date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30).Date).ToString("yyyy-MM-dd"), amount = 1000 } } }
        };

        var created = await client.PostAsJsonAsync("/v1/issuances", req);
        created.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdBody = await created.Content.ReadFromJsonAsync<IssuanceResponse>();
        createdBody.Should().NotBeNull();

        var get = await client.GetAsync($"/v1/issuances/{createdBody!.Id}");
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        var got = await get.Content.ReadFromJsonAsync<IssuanceResponse>();
        got!.Id.Should().Be(createdBody.Id);
        got.ScheduleJson.Should().NotBeNull();
    }

    [Fact]
    public async Task Publish_NonExistent_Should_Return_404()
    {
        var client = CreateClient();
        var res = await client.PostAsync($"/v1/issuances/{Guid.NewGuid()}/publish", content: null);
        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_Invalid_Should_Return_400()
    {
        var client = CreateClient();
        var badReq = new CreateIssuanceRequest
        {
            AssetId = Guid.Empty, // invalid
            IssuerId = Guid.NewGuid(),
            TotalAmount = 0m, // invalid
            Nominal = -5m, // invalid
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.Date) // invalid (not after)
        };

        var response = await client.PostAsJsonAsync("/v1/issuances", badReq);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Publish_Then_Close_Should_Succeed()
    {
        var client = CreateClient();
        var req = new CreateIssuanceRequest
        {
            AssetId = Guid.NewGuid(),
            IssuerId = Guid.NewGuid(),
            TotalAmount = 1000m,
            Nominal = 10m,
            IssueDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            MaturityDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30).Date)
        };

        var created = await client.PostAsJsonAsync("/v1/issuances", req);
        var issuance = await created.Content.ReadFromJsonAsync<IssuanceResponse>();

        var publish = await client.PostAsync($"/v1/issuances/{issuance!.Id}/publish", null);
        publish.StatusCode.Should().Be(HttpStatusCode.OK);

        var close = await client.PostAsync($"/v1/issuances/{issuance!.Id}/close", null);
        close.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace DB with InMemory
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<IssuanceDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            services.AddDbContext<IssuanceDbContext>(options => options.UseInMemoryDatabase("IssuanceTestsDb"));

            // Override authentication to always succeed with issuer role
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(JwtBearerDefaults.AuthenticationScheme, options => { });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("role:issuer", p => p.RequireRole("issuer"));
                options.AddPolicy("role:any-auth", p => p.RequireAuthenticatedUser());
            });
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
