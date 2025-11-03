using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace OIS.Registry.Services;

public interface IBankNominalService
{
    Task<string> ReserveFundsAsync(Guid investorId, decimal amount, string idempotencyKey, CancellationToken ct);
}

public class BankNominalServiceClient : IBankNominalService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankNominalServiceClient> _logger;
    private readonly string _baseUrl;

    public BankNominalServiceClient(
        HttpClient httpClient,
        ILogger<BankNominalServiceClient> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["BankNominal:BaseUrl"] ?? "http://bank-nominal:8080";
    }

    public async Task<string> ReserveFundsAsync(Guid investorId, decimal amount, string idempotencyKey, CancellationToken ct)
    {
        var request = new
        {
            investorId = investorId.ToString(),
            amount = amount,
            idempotencyKey = idempotencyKey
        };

        using var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

        var response = await _httpClient.PostAsync($"{_baseUrl}/nominal/reserve", content, ct);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Bank nominal reserve failed: {StatusCode} {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Failed to reserve funds: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<ReserveFundsResponse>(cancellationToken: ct);
        return result?.TransferId ?? throw new InvalidOperationException("No transfer ID returned");
    }

    private record ReserveFundsResponse
    {
        public string? TransferId { get; init; }
    }
}

