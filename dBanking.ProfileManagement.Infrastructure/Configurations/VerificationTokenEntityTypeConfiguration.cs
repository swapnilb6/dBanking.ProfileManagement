using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.DbContext.Configurations
{
    public class VerificationTokenEntityTypeConfiguration : IEntityTypeConfiguration<VerificationToken>
    {

        public void Configure(EntityTypeBuilder<VerificationToken> b)
        {
            b.ToTable("verification_tokens");
            b.HasKey(v => v.VerificationId);

            b.Property(v => v.Type).HasConversion<int>();
            b.Property(v => v.Status).HasConversion<int>();

            b.Property(v => v.ChannelValue).HasMaxLength(320).IsRequired(); // email or phoneE164
            b.Property(v => v.TokenHash).HasMaxLength(256).IsRequired();
            b.Property(v => v.OtpSalt).HasMaxLength(64);

            b.Property(v => v.AttemptCount).IsRequired();
            b.Property(v => v.MaxAttempts).IsRequired();

            b.Property(v => v.CreatedAt).IsRequired();
            b.Property(v => v.ExpiresAt).IsRequired();

            b.HasIndex(v => v.CustomerId);
            b.HasIndex(v => v.TokenHash);

            b.HasOne<Profile>()
             .WithMany()
             .HasForeignKey(v => v.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);

            // ----- Seed -----
            var c1 = new Guid("8C2F4E8E-7C1C-4C5B-8E2D-9F3A7D1B2C01");
            var c2 = new Guid("2A4B6C8D-1E2F-4A5B-9C0D-1E2F3A4B5C02");
            var c3 = new Guid("3B5C7D9E-2F3A-4B5C-8D9E-0F1A2B3C4D03");
            var now = new DateTimeOffset(2026, 02, 01, 0, 0, 0, TimeSpan.Zero);

            b.HasData(
                new VerificationToken
                {
                    VerificationId = new Guid("AAAAAAA1-AAAA-AAAA-AAAA-AAAAAAAAAAA1"),
                    CustomerId = c1,
                    Type = VerificationType.EmailLink,
                    ChannelValue = "swapnil.bawankar@example.com",
                    TokenHash = "hash_verify_swapnil_001",
                    OtpSalt = null,
                    Status = VerificationStatus.Pending,
                    ExpiresAt = now.AddDays(1),
                    AttemptCount = 0,
                    MaxAttempts = 5,
                    CorrelationId = "corr-verify-swapnil-001",
                    CreatedAt = now,
                    VerifiedAt = null,
                    FailureReason = null
                },
                new VerificationToken
                {
                    VerificationId = new Guid("AAAAAAA2-AAAA-AAAA-AAAA-AAAAAAAAAAA2"),
                    CustomerId = c2,
                    Type = VerificationType.SmsOtp,
                    ChannelValue = "+919822011122",
                    TokenHash = "hash_reset_isha_otp_001",
                    OtpSalt = "salt01",
                    Status = VerificationStatus.Pending,
                    ExpiresAt = now.AddHours(2),
                    AttemptCount = 1,
                    MaxAttempts = 5,
                    CorrelationId = "corr-reset-isha-001",
                    CreatedAt = now,
                    VerifiedAt = null,
                    FailureReason = null
                },
                new VerificationToken
                {
                    VerificationId = new Guid("AAAAAAA3-AAAA-AAAA-AAAA-AAAAAAAAAAA3"),
                    CustomerId = c3,
                    Type = VerificationType.EmailLink,
                    ChannelValue = "rahul.kulkarni@example.com",
                    TokenHash = "hash_verify_rahul_001",
                    OtpSalt = null,
                    Status = VerificationStatus.Pending,
                    ExpiresAt = now.AddDays(1),
                    AttemptCount = 0,
                    MaxAttempts = 5,
                    CorrelationId = "corr-verify-rahul-001",
                    CreatedAt = now,
                    VerifiedAt = null,
                    FailureReason = null
                }
            );
        }
    }

}