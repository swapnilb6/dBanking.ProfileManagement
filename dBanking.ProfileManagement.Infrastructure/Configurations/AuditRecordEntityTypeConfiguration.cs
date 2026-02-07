using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.DbContext.Configurations
{
    public class AuditRecordEntityTypeConfiguration : IEntityTypeConfiguration<AuditRecord>
    {

        public void Configure(EntityTypeBuilder<AuditRecord> b)
        {
            b.ToTable("audit_records");
            b.HasKey(a => a.AuditId);

            b.Property(a => a.Entity).HasMaxLength(64).IsRequired();
            b.Property(a => a.EntityId).HasMaxLength(64).IsRequired();
            b.Property(a => a.Operation).HasMaxLength(32).IsRequired();

            b.Property(a => a.ActorId).HasMaxLength(128).IsRequired();
            b.Property(a => a.ActorRole).HasConversion<int>().IsRequired();
            b.Property(a => a.SourceChannel).HasConversion<int>().IsRequired();

            b.Property(a => a.ReasonCode).HasMaxLength(64);
            b.Property(a => a.IpAddress).HasMaxLength(64);
            b.Property(a => a.UserAgent).HasMaxLength(256);
            b.Property(a => a.CorrelationId).HasMaxLength(128);

            b.Property(a => a.OldValueJson).IsRequired();
            b.Property(a => a.NewValueJson).IsRequired();
            b.Property(a => a.ChangedFieldsCsv).IsRequired();

            b.Property(a => a.Timestamp).IsRequired();

            b.HasIndex(a => a.CustomerId);
            b.HasIndex(a => new { a.Entity, a.EntityId });
            b.HasIndex(a => new { a.Operation, a.Timestamp });

            // ----- Seed -----
            var c1 = new Guid("8C2F4E8E-7C1C-4C5B-8E2D-9F3A7D1B2C01");
            var c2 = new Guid("2A4B6C8D-1E2F-4A5B-9C0D-1E2F3A4B5C02");
            var when = new DateTimeOffset(2026, 02, 01, 0, 0, 0, TimeSpan.Zero);

            b.HasData(
                new AuditRecord
                {
                    AuditId = new Guid("B0000001-0000-0000-0000-000000000001"),
                    CustomerId = c1,
                    Entity = "Contacts",
                    EntityId = "contact",
                    Operation = "Create",
                    OldValueJson = "{}",
                    NewValueJson = "{\"email\":\"swapnil.bawankar@example.com\",\"phoneE164\":\"+919876543210\"}",
                    ChangedFieldsCsv = "email,phoneE164",
                    ActorId = "system",
                    ActorRole = ActorRole.System,
                    SourceChannel = SourceChannel.System,
                    ReasonCode = null,
                    IpAddress = "127.0.0.1",
                    UserAgent = "seed/1.0",
                    CorrelationId = "seed-corr-0001",
                    Timestamp = when
                },
                new AuditRecord
                {
                    AuditId = new Guid("B0000002-0000-0000-0000-000000000002"),
                    CustomerId = c1,
                    Entity = "Address",
                    EntityId = "11111111-1111-1111-1111-111111111111",
                    Operation = "Create",
                    OldValueJson = "{}",
                    NewValueJson = "{\"city\":\"Pune\",\"isPrimary\":true}",
                    ChangedFieldsCsv = "city,isPrimary",
                    ActorId = "system",
                    ActorRole = ActorRole.System,
                    SourceChannel = SourceChannel.System,
                    ReasonCode = null,
                    IpAddress = "127.0.0.1",
                    UserAgent = "seed/1.0",
                    CorrelationId = "seed-corr-0002",
                    Timestamp = when
                },
                new AuditRecord
                {
                    AuditId = new Guid("B0000003-0000-0000-0000-000000000003"),
                    CustomerId = c2,
                    Entity = "Contacts",
                    EntityId = "contact",
                    Operation = "Create",
                    OldValueJson = "{}",
                    NewValueJson = "{\"email\":\"anita.deshmukh@example.com\",\"phoneE164\":\"+919822011122\"}",
                    ChangedFieldsCsv = "email,phoneE164",
                    ActorId = "system",
                    ActorRole = ActorRole.System,
                    SourceChannel = SourceChannel.System,
                    ReasonCode = "onboarding",
                    IpAddress = "127.0.0.1",
                    UserAgent = "seed/1.0",
                    CorrelationId = "seed-corr-0003",
                    Timestamp = when
                }
            );
        }
    }
}