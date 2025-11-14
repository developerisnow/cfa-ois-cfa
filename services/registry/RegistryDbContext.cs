using Microsoft.EntityFrameworkCore;

namespace OIS.Registry;

public class RegistryDbContext : DbContext
{
    public RegistryDbContext(DbContextOptions<RegistryDbContext> options) : base(options) { }

    public DbSet<WalletEntity> Wallets => Set<WalletEntity>();
    public DbSet<HoldingEntity> Holdings => Set<HoldingEntity>();
    public DbSet<OrderEntity> Orders => Set<OrderEntity>();
    public DbSet<TransactionEntity> Transactions => Set<TransactionEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<WalletEntity>(entity =>
        {
            entity.ToTable("wallets");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.OwnerType)
                .HasColumnName("owner_type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.OwnerId)
                .HasColumnName("owner_id")
                .IsRequired();

            entity.Property(e => e.Balance)
                .HasColumnName("balance")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.Blocked)
                .HasColumnName("blocked")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.HasIndex(e => new { e.OwnerType, e.OwnerId }).IsUnique();
        });

        modelBuilder.Entity<HoldingEntity>(entity =>
        {
            entity.ToTable("holdings");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.InvestorId)
                .HasColumnName("investor_id")
                .IsRequired();

            entity.Property(e => e.IssuanceId)
                .HasColumnName("issuance_id")
                .IsRequired();

            entity.Property(e => e.Quantity)
                .HasColumnName("quantity")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.HasIndex(e => new { e.InvestorId, e.IssuanceId }).IsUnique();
        });

        modelBuilder.Entity<OrderEntity>(entity =>
        {
            entity.ToTable("orders");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.InvestorId)
                .HasColumnName("investor_id")
                .IsRequired();

            entity.Property(e => e.IssuanceId)
                .HasColumnName("issuance_id")
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.IdemKey)
                .HasColumnName("idem_key")
                .HasMaxLength(255);

            entity.Property(e => e.WalletId)
                .HasColumnName("wallet_id");

            entity.Property(e => e.DltTxHash)
                .HasColumnName("dlt_tx_hash")
                .HasMaxLength(64);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(e => e.ConfirmedAt)
                .HasColumnName("confirmed_at");

            entity.Property(e => e.FailureReason)
                .HasColumnName("failure_reason");

            entity.HasIndex(e => e.IdemKey).IsUnique()
                .HasFilter("\"idem_key\" IS NOT NULL");
            entity.HasIndex(e => e.InvestorId);
            entity.HasIndex(e => e.IssuanceId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<TransactionEntity>(entity =>
        {
            entity.ToTable("tx");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.Type)
                .HasColumnName("type")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.FromWalletId)
                .HasColumnName("from_wallet_id");

            entity.Property(e => e.ToWalletId)
                .HasColumnName("to_wallet_id");

            entity.Property(e => e.IssuanceId)
                .HasColumnName("issuance_id");

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.DltTxHash)
                .HasColumnName("dlt_tx_hash")
                .HasMaxLength(64);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.ConfirmedAt)
                .HasColumnName("confirmed_at");

            entity.HasIndex(e => e.IssuanceId);
            entity.HasIndex(e => e.FromWalletId);
            entity.HasIndex(e => e.ToWalletId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.Topic)
                .HasColumnName("topic")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(e => e.Payload)
                .HasColumnName("payload")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.ProcessedAt)
                .HasColumnName("processed_at");

            entity.HasIndex(e => new { e.ProcessedAt, e.CreatedAt });
        });
    }
}

public class WalletEntity
{
    public Guid Id { get; set; }
    public string OwnerType { get; set; } = string.Empty; // "individual" or "legal_entity"
    public Guid OwnerId { get; set; }
    public decimal Balance { get; set; }
    public decimal Blocked { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class HoldingEntity
{
    public Guid Id { get; set; }
    public Guid InvestorId { get; set; }
    public Guid IssuanceId { get; set; }
    public decimal Quantity { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderEntity
{
    public Guid Id { get; set; }
    public Guid InvestorId { get; set; }
    public Guid IssuanceId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "created"; // created, reserved, paid, failed, cancelled
    public string? IdemKey { get; set; }
    public Guid? WalletId { get; set; }
    public string? DltTxHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? FailureReason { get; set; }
}

public class TransactionEntity
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // transfer, redeem, issue
    public Guid? FromWalletId { get; set; }
    public Guid? ToWalletId { get; set; }
    public Guid? IssuanceId { get; set; }
    public decimal Amount { get; set; }
    public string? DltTxHash { get; set; }
    public string Status { get; set; } = "pending"; // pending, confirmed, failed
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

