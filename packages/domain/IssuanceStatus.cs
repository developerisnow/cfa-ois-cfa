namespace OIS.Domain;

/// <summary>
/// Issuance status enumeration
/// </summary>
public enum IssuanceStatus
{
    Draft = 0,
    Published = 1,
    Closed = 2,
    Redeemed = 3
}

/// <summary>
/// Status transition rules and helper methods
/// </summary>
public static class IssuanceStatusExtensions
{
    public static bool CanTransitionTo(this IssuanceStatus from, IssuanceStatus to)
    {
        return (from, to) switch
        {
            (IssuanceStatus.Draft, IssuanceStatus.Published) => true,
            (IssuanceStatus.Published, IssuanceStatus.Closed) => true,
            (IssuanceStatus.Closed, IssuanceStatus.Redeemed) => true,
            _ => false
        };
    }

    public static string ToStringValue(this IssuanceStatus status) => status.ToString().ToLowerInvariant();

    public static IssuanceStatus FromString(string value)
    {
        return value?.ToLowerInvariant() switch
        {
            "draft" => IssuanceStatus.Draft,
            "published" => IssuanceStatus.Published,
            "closed" => IssuanceStatus.Closed,
            "redeemed" => IssuanceStatus.Redeemed,
            _ => throw new ArgumentException($"Unknown status: {value}", nameof(value))
        };
    }
}

