using Microsoft.EntityFrameworkCore;

namespace OIS.Settlement;

public class SettlementDbContext : DbContext
{
    public SettlementDbContext(DbContextOptions<SettlementDbContext> options) : base(options) { }

    public DbSet<PayoutBatchEntity> PayoutBatches => Set<PayoutBatchEntity>();
    public DbSet<PayoutItemEntity> PayoutItems => Set<PayoutItemEntity>();
    public DbSet<ReconciliationLogEntity> ReconciliationLogs => Set<ReconciliationLogEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedMessage> ProcessedMessages => Set<ProcessedMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PayoutBatchEntity>(entity =>
        {
            entity.ToTable("payouts_batch");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.RunDate)
                .HasColumnName("run_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(e => e.IssuanceId)
                .HasColumnName("issuance_id");

            entity.Property(e => e.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.IdemKey)
                .HasColumnName("idem_key")
                .HasMaxLength(255);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasIndex(e => e.IdemKey).IsUnique()
                .HasFilter("\"idem_key\" IS NOT NULL");
            entity.HasIndex(e => e.RunDate);
            entity.HasIndex(e => e.IssuanceId);
            entity.HasIndex(e => e.Status);
        });

        modelBuilder.Entity<PayoutItemEntity>(entity =>
        {
            entity.ToTable("payouts_item");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.BatchId)
                .HasColumnName("batch_id")
                .IsRequired();

            entity.Property(e => e.InvestorId)
                .HasColumnName("investor_id")
                .IsRequired();

            entity.Property(e => e.Amount)
                .HasColumnName("amount")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.BankRef)
                .HasColumnName("bank_ref")
                .HasMaxLength(255);

            entity.Property(e => e.DltTxHash)
                .HasColumnName("dlt_tx_hash")
                .HasMaxLength(64);

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.FailureReason)
                .HasColumnName("failure_reason");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasIndex(e => e.BatchId);
            entity.HasIndex(e => e.InvestorId);
            entity.HasIndex(e => e.Status);

            entity.HasOne<PayoutBatchEntity>()
                .WithMany()
                .HasForeignKey(e => e.BatchId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReconciliationLogEntity>(entity =>
        {
            entity.ToTable("reconciliation_log");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.BatchId)
                .HasColumnName("batch_id")
                .IsRequired();

            entity.Property(e => e.PayloadJson)
                .HasColumnName("payload_json")
                .HasColumnType("jsonb")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasIndex(e => e.BatchId);
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

        modelBuilder.Entity<ProcessedMessage>(entity =>
        {
            entity.ToTable("inbox_processed");
            entity.HasKey(e => e.MessageId);
            entity.Property(e => e.MessageId)
                .HasColumnName("message_id")
                .HasMaxLength(128);
            entity.Property(e => e.Consumer)
                .HasColumnName("consumer")
                .HasMaxLength(128);
            entity.Property(e => e.ProcessedAt)
                .HasColumnName("processed_at");
            entity.HasIndex(e => new { e.Consumer, e.ProcessedAt });
        });
    }
}

public class PayoutBatchEntity
{
    public Guid Id { get; set; }
    public DateOnly RunDate { get; set; }
    public Guid? IssuanceId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "pending"; // pending, processing, completed, failed
    public string? IdemKey { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PayoutItemEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public Guid InvestorId { get; set; }
    public decimal Amount { get; set; }
    public string? BankRef { get; set; }
    public string? DltTxHash { get; set; }
    public string Status { get; set; } = "pending"; // pending, completed, failed
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReconciliationLogEntity
{
    public Guid Id { get; set; }
    public Guid BatchId { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class ProcessedMessage
{
    public string MessageId { get; set; } = string.Empty;
    public string Consumer { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}

