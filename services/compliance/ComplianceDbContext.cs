using Microsoft.EntityFrameworkCore;

namespace OIS.Compliance;

public class ComplianceDbContext : DbContext
{
    public ComplianceDbContext(DbContextOptions<ComplianceDbContext> options) : base(options) { }

    public DbSet<InvestorComplianceEntity> InvestorsCompliance => Set<InvestorComplianceEntity>();
    public DbSet<ComplaintEntity> Complaints => Set<ComplaintEntity>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InvestorComplianceEntity>(entity =>
        {
            entity.ToTable("investors_compliance");
            entity.HasKey(e => e.InvestorId);

            entity.Property(e => e.InvestorId)
                .HasColumnName("investor_id")
                .ValueGeneratedNever();

            entity.Property(e => e.Kyc)
                .HasColumnName("kyc")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.QualificationTier)
                .HasColumnName("qualification_tier")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.QualLimit)
                .HasColumnName("qual_limit")
                .HasPrecision(20, 8);

            entity.Property(e => e.QualUsed)
                .HasColumnName("qual_used")
                .HasPrecision(20, 8);

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .IsRequired();
        });

        modelBuilder.Entity<ComplaintEntity>(entity =>
        {
            entity.ToTable("complaints");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .ValueGeneratedNever();

            entity.Property(e => e.InvestorId)
                .HasColumnName("investor_id");

            entity.Property(e => e.Category)
                .HasColumnName("category")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.Text)
                .HasColumnName("text")
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(e => e.SlaDue)
                .HasColumnName("sla_due");

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.Property(e => e.ResolvedAt)
                .HasColumnName("resolved_at");

            entity.Property(e => e.IdemKey)
                .HasColumnName("idem_key")
                .HasMaxLength(255);

            entity.HasIndex(e => e.IdemKey).IsUnique()
                .HasFilter("\"idem_key\" IS NOT NULL");
            entity.HasIndex(e => e.InvestorId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Category);
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

public class InvestorComplianceEntity
{
    public Guid InvestorId { get; set; }
    public string Kyc { get; set; } = "pending"; // pass, fail, pending, review
    public string QualificationTier { get; set; } = "unqualified"; // unqualified, qualified, professional
    public decimal? QualLimit { get; set; }
    public decimal? QualUsed { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ComplaintEntity
{
    public Guid Id { get; set; }
    public Guid? InvestorId { get; set; }
    public string Category { get; set; } = string.Empty; // fraud, service, technical, other
    public string Text { get; set; } = string.Empty;
    public string Status { get; set; } = "open"; // open, in_progress, resolved, closed
    public DateTime? SlaDue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? IdemKey { get; set; }
}

public class OutboxMessage
{
    public Guid Id { get; set; }
    public string Topic { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

