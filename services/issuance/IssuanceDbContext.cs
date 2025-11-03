using Microsoft.EntityFrameworkCore;
using OIS.Domain;
using System.Text.Json;

namespace OIS.Issuance;

public class IssuanceDbContext : DbContext
{
    public IssuanceDbContext(DbContextOptions<IssuanceDbContext> options) : base(options) { }

    public DbSet<IssuanceEntity> Issuances => Set<IssuanceEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IssuanceEntity>(entity =>
        {
            entity.ToTable("issuances");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.AssetId)
                .HasColumnName("asset_id")
                .IsRequired();

            entity.Property(e => e.IssuerId)
                .HasColumnName("issuer_id")
                .IsRequired();

            entity.Property(e => e.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.Nominal)
                .HasColumnName("nominal")
                .HasPrecision(20, 8)
                .IsRequired();

            entity.Property(e => e.IssueDate)
                .HasColumnName("issue_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(e => e.MaturityDate)
                .HasColumnName("maturity_date")
                .HasColumnType("date")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToStringValue(),
                    v => IssuanceStatusExtensions.FromString(v))
                .IsRequired();

            entity.Property(e => e.ScheduleJson)
                .HasColumnName("schedule_json")
                .HasColumnType("jsonb");

            entity.Property(e => e.DltTxHash)
                .HasColumnName("dlt_tx_hash")
                .HasMaxLength(64);

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();

            entity.Property(e => e.PublishedAt)
                .HasColumnName("published_at");

            entity.Property(e => e.ClosedAt)
                .HasColumnName("closed_at");

            entity.HasIndex(e => e.AssetId);
            entity.HasIndex(e => e.IssuerId);
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

public class IssuanceEntity
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public Guid IssuerId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Nominal { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly MaturityDate { get; set; }
    public IssuanceStatus Status { get; set; }
    public string? ScheduleJson { get; set; }
    public string? DltTxHash { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

