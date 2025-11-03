using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace OIS.Settlement.Services;

public interface IBankNominalClient
{
    Task<BatchPayoutResponse> ExecuteBatchPayoutAsync(BatchPayoutRequest request, string idempotencyKey, CancellationToken ct);
}

public record BatchPayoutRequest
{
    public Guid BatchId { get; init; }
    public DateOnly RunDate { get; init; }
    public IReadOnlyList<PayoutItemRequest> Items { get; init; } = Array.Empty<PayoutItemRequest>();
}

public record PayoutItemRequest
{
    public Guid ItemId { get; init; }
    public Guid InvestorId { get; init; }
    public decimal Amount { get; init; }
}

public record BatchPayoutResponse
{
    public string BatchRef { get; init; } = string.Empty;
    public IReadOnlyList<ItemPayoutResponse> Items { get; init; } = Array.Empty<ItemPayoutResponse>();
}

public record ItemPayoutResponse
{
    public Guid ItemId { get; init; }
    public string? BankRef { get; init; }
    public string Status { get; init; } = string.Empty; // completed, failed
    public string? FailureReason { get; init; }
}

public class BankNominalClient : IBankNominalClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BankNominalClient> _logger;
    private readonly string _baseUrl;

    public BankNominalClient(HttpClient httpClient, ILogger<BankNominalClient> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _baseUrl = configuration["BankNominal:BaseUrl"] ?? "http://bank-nominal:8080";
    }

    public async Task<BatchPayoutResponse> ExecuteBatchPayoutAsync(BatchPayoutRequest request, string idempotencyKey, CancellationToken ct)
    {
        using var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey);

        var response = await _httpClient.PostAsync($"{_baseUrl}/nominal/payouts/batch", content, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("Bank nominal batch payout failed: {StatusCode} {Error}", response.StatusCode, error);
            throw new InvalidOperationException($"Failed to execute batch payout: {response.StatusCode}");
        }

        var result = await response.Content.ReadFromJsonAsync<BatchPayoutResponse>(cancellationToken: ct);
        return result ?? throw new InvalidOperationException("No response from bank nominal");
    }
}

