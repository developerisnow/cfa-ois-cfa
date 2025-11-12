namespace OIS.Compliance.Services;

public interface IWatchlistsService
{
    Task<WatchlistCheckResult> CheckAsync(Guid investorId, CancellationToken ct);
}

public record WatchlistCheckResult
{
    public bool Matched { get; init; }
    public string? Reason { get; init; }
    public string Severity { get; init; } = "low"; // low, medium, high, critical
}

public class WatchlistsServiceStub : IWatchlistsService
{
    private readonly ILogger<WatchlistsServiceStub> _logger;

    public WatchlistsServiceStub(ILogger<WatchlistsServiceStub> logger)
    {
        _logger = logger;
    }

    public Task<WatchlistCheckResult> CheckAsync(Guid investorId, CancellationToken ct)
    {
        // Deterministic demo: check last byte of GUID
        var lastByte = investorId.ToByteArray().Last();
        var matched = lastByte % 10 == 0; // ~10% match rate

        _logger.LogInformation("Watchlists check for investor {Investor}: matched={Matched}", OIS.Domain.Security.MaskGuid(investorId), matched);

        return Task.FromResult(new WatchlistCheckResult
        {
            Matched = matched,
            Reason = matched ? "Demo watchlist match" : null,
            Severity = matched ? "medium" : "low"
        });
    }
}

