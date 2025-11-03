namespace OIS.Domain;

/// <summary>
/// Value object representing monetary amount
/// </summary>
public sealed record Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    private Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        
        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required", nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    public static Money Create(decimal amount, string currency = "RUB") => new(amount, currency);

    public static Money Zero(string currency = "RUB") => new(0, currency);

    public static Money operator +(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");

        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot subtract money with different currencies");

        if (left.Amount < right.Amount)
            throw new InvalidOperationException("Result cannot be negative");

        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static bool operator >(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");
        return left.Amount > right.Amount;
    }

    public static bool operator <(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new InvalidOperationException("Cannot compare money with different currencies");
        return left.Amount < right.Amount;
    }

    public static bool operator >=(Money left, Money right) => !(left < right);
    public static bool operator <=(Money left, Money right) => !(left > right);
}

