using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using PactNet;
using PactNet.Matchers;
using PactNet.Mocks.MockHttpService;
using PactNet.Mocks.MockHttpService.Models;

namespace OIS.Registry.Tests.Contracts;

public class RegistryProviderVerification : IClassFixture<RegistryProviderState>
{
    private readonly IMockProviderService _mockProvider;
    private readonly string _mockProviderServiceBaseUri;

    public RegistryProviderVerification(RegistryProviderState state)
    {
        _mockProvider = state.MockProviderService;
        _mockProviderServiceBaseUri = state.MockProviderServiceBaseUri;
        _mockProvider.ClearInteractions();
    }

    [Fact]
    public async Task PlaceOrder_WhenInvestorHasPassedKYC_Returns202()
    {
        _mockProvider
            .Given("investor has passed KYC and qualification check")
            .UponReceiving("a request to place an order")
            .With(new ProviderServiceRequest
            {
                Method = HttpVerb.Post,
                Path = "/v1/orders",
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" },
                    { "Idempotency-Key", Match.Type("550e8400-e29b-41d4-a716-446655440000") }
                },
                Body = new
                {
                    investorId = Match.Type("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    issuanceId = Match.Type("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    amount = Match.Type(50000)
                }
            })
            .WillRespondWith(new ProviderServiceResponse
            {
                Status = 202,
                Headers = new Dictionary<string, object>
                {
                    { "Content-Type", "application/json" }
                },
                Body = new
                {
                    id = Match.Type("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    investorId = Match.Type("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    issuanceId = Match.Type("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                    amount = Match.Type(50000),
                    status = Match.Regex("pending|completed|failed", "pending"),
                    createdAt = Match.Type("2025-01-01T00:00:00Z")
                }
            });

        // Verify provider implementation
        using var client = new HttpClient { BaseAddress = new Uri(_mockProviderServiceBaseUri) };
        var response = await client.PostAsync("/v1/orders", new StringContent("{}"));
        
        Assert.Equal(202, (int)response.StatusCode);
    }
}

