namespace OIS.Domain;

/// <summary>
/// Value object for Issuance identifier
/// </summary>
public sealed record IssuanceId
{
    public Guid Value { get; }

    private IssuanceId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Issuance ID cannot be empty", nameof(value));

        Value = value;
    }

    public static IssuanceId Create() => new(Guid.NewGuid());

    public static IssuanceId From(Guid value) => new(value);

    public static implicit operator Guid(IssuanceId id) => id.Value;
    public static implicit operator IssuanceId(Guid value) => From(value);
}

