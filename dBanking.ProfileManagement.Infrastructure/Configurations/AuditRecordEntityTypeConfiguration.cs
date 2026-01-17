using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.DbContext.Configurations
{
    public class AuditRecordEntityTypeConfiguration : IEntityTypeConfiguration<AuditRecord>
    {
        public void Configure(EntityTypeBuilder<AuditRecord> builder)
        {
            builder.ToTable("audit_record");
            builder.HasKey(a => a.AuditId);

            builder.Property(a => a.CustomerId).IsRequired();
            builder.Property(a => a.Entity).HasMaxLength(64).IsRequired();
            builder.Property(a => a.EntityId).HasMaxLength(64).IsRequired();
            builder.Property(a => a.Operation).HasMaxLength(32).IsRequired();

            // Store snapshots as JSONB
            builder.Property(a => a.OldValueJson)
                   .HasColumnType("jsonb")
                   .HasDefaultValueSql("'{}'::jsonb")
                   .IsRequired();

            builder.Property(a => a.NewValueJson)
                   .HasColumnType("jsonb")
                   .HasDefaultValueSql("'{}'::jsonb")
                   .IsRequired();

            builder.Property(a => a.ChangedFieldsCsv).HasMaxLength(512);

            builder.Property(a => a.ActorId).HasMaxLength(64).IsRequired();
            builder.Property(a => a.ActorRole).HasConversion<int>().IsRequired();
            builder.Property(a => a.SourceChannel).HasConversion<int>().IsRequired();

            builder.Property(a => a.ReasonCode).HasMaxLength(64);
            builder.Property(a => a.IpAddress).HasMaxLength(64);
            builder.Property(a => a.UserAgent).HasMaxLength(256);
            builder.Property(a => a.CorrelationId).HasMaxLength(100);

            builder.Property(a => a.Timestamp).IsRequired();

            builder.HasIndex(a => new { a.CustomerId, a.Timestamp })
                   .HasDatabaseName("ix_audit_customer_ts");
        }
    }
}