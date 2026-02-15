using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.DbContext.Configurations
{

    public sealed class AuditRecordEntityTypeConfiguration : IEntityTypeConfiguration<AuditRecord>
    {
        public void Configure(EntityTypeBuilder<AuditRecord> b)
        {
            // Table
            b.ToTable("audit_records");

            // Key
            b.HasKey(a => a.Id);

            // Required / Max lengths
            b.Property(a => a.CustomerId)
             .IsRequired();

            b.Property(a => a.EntityName)
             .IsRequired()
             .HasMaxLength(128);

            b.Property(a => a.EntityId)
             .IsRequired()
             .HasMaxLength(64);

            b.Property(a => a.Action)
             .IsRequired()
             .HasMaxLength(32);

            b.Property(a => a.Actor)
             .HasMaxLength(128);

            b.Property(a => a.Channel)
             .HasMaxLength(64);

            // Timestamps
            b.Property(a => a.ChangedAt)
             .HasDefaultValueSql("now()");

            // JSONB snapshots (entity uses string? backing)
            b.Property(a => a.OldJson)
             .HasColumnType("jsonb");

            b.Property(a => a.NewJson)
             .HasColumnType("jsonb");

            // Optional fields
            b.Property(a => a.ChangedFieldsCsv);
            b.Property(a => a.CorrelationId);

            // Indexes (match DB script)
            b.HasIndex(a => new { a.CustomerId, a.ChangedAt })
             .HasDatabaseName("ix_audit_customer_time")
#if NET8_0_OR_GREATER
             .IsDescending(false, true);
#else
             ;
#endif

            b.HasIndex(a => new { a.EntityName, a.EntityId })
             .HasDatabaseName("ix_audit_entity");
        }
    }
}