using System.Text.Json;

namespace OIS.Issuance.Services;

public interface IOutboxService
{
    Task AddAsync(string topic, object payload, CancellationToken ct);
}

public class OutboxService : IOutboxService
{
    private readonly IssuanceDbContext _db;

    public OutboxService(IssuanceDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(string topic, object payload, CancellationToken ct)
    {
        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Topic = topic,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow
        };

        _db.OutboxMessages.Add(message);
    }
}

