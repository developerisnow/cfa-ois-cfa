namespace OIS.Domain;

/// <summary>
/// Payout schedule item
/// </summary>
public sealed record ScheduleItem
{
    public DateOnly Date { get; }
    public decimal Amount { get; }
    public string Description { get; }

    public ScheduleItem(DateOnly date, decimal amount, string description = "")
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive", nameof(amount));

        Date = date;
        Amount = amount;
        Description = description ?? string.Empty;
    }
}

/// <summary>
/// Payout schedule (collection of schedule items)
/// </summary>
public sealed record PayoutSchedule
{
    public IReadOnlyList<ScheduleItem> Items { get; }

    public PayoutSchedule(IEnumerable<ScheduleItem> items)
    {
        Items = items?.ToList().AsReadOnly() ?? throw new ArgumentNullException(nameof(items));
        
        if (Items.Count == 0)
            throw new ArgumentException("Schedule must have at least one item", nameof(items));
    }

    public decimal TotalAmount => Items.Sum(i => i.Amount);
}

