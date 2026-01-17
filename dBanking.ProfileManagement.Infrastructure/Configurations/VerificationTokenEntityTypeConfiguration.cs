using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.DbContext.Configurations
{
    public class VerificationTokenEntityTypeConfiguration : IEntityTypeConfiguration<VerificationToken>
    {
        public void Configure(EntityTypeBuilder<VerificationToken> builder)
        {
            builder.ToTable("verification_token");
            builder.HasKey(v => v.VerificationId);

            builder.Property(v => v.CustomerId).IsRequired();

            builder.Property(v => v.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(v => v.ChannelValue)
                .HasMaxLength(254) // email max; phone fits well under this
                .IsRequired();

            builder.Property(v => v.TokenHash)
                .HasMaxLength(128)
                .IsRequired();

            builder.Property(v => v.OtpSalt)
                .HasMaxLength(64);

            builder.Property(v => v.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(v => v.ExpiresAt).IsRequired();
            builder.Property(v => v.AttemptCount).IsRequired();
            builder.Property(v => v.MaxAttempts).IsRequired();

            builder.Property(v => v.CorrelationId).HasMaxLength(100);
            builder.Property(v => v.CreatedAt).IsRequired();
            builder.Property(v => v.VerifiedAt);
            builder.Property(v => v.FailureReason).HasMaxLength(200);

            builder.HasIndex(v => new { v.CustomerId, v.Type, v.Status })
                .HasDatabaseName("ix_verif_customer_type_status");

            builder.HasIndex(v => v.ExpiresAt)
                .HasDatabaseName("ix_verif_expiry");

            builder.HasIndex(v => v.CorrelationId)
                .HasDatabaseName("ix_verif_corr");
        }
    }
}