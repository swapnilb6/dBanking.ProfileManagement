using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.Configurations
{
    public class ProfileEntityTypeConfiguration : IEntityTypeConfiguration<Profile>
    {

        public void Configure(EntityTypeBuilder<Profile> b)
        {
            b.ToTable("profiles");
            b.HasKey(p => p.CustomerId);

            // Concurrency (PostgreSQL doesn't have rowversion; this just acts as a token)
            b.Property(p => p.RowVersion).IsConcurrencyToken();

            b.Property(p => p.CreatedAt).IsRequired();
            b.Property(p => p.UpdatedAt).IsRequired();

            // ----- Contact (owned, separate table) -----
            b.OwnsOne(p => p.Contact, owned =>
            {
                owned.ToTable("contacts");
                owned.WithOwner().HasForeignKey("customer_id"); // shadow FK named exactly "customer_id"
                owned.HasKey("customer_id");

                // If you enabled citext:
                // owned.Property(x => x.Email).HasMaxLength(320).HasColumnType("citext");
                // If not using citext, keep it varchar/text:
                owned.Property(x => x.Email).HasMaxLength(320);

                owned.Property(x => x.PhoneE164).HasMaxLength(32);
                owned.Property(x => x.EmailStatus).HasConversion<int>();
                owned.Property(x => x.PhoneStatus).HasConversion<int>();
                owned.HasIndex(x => x.Email);
                owned.HasIndex(x => x.PhoneE164);

                // Seed owned Contact (use the shadow FK property name: "customer_id")
                var now = new DateTimeOffset(2026, 02, 01, 0, 0, 0, TimeSpan.Zero);

                owned.HasData(
                    new
                    {
                        customer_id = new Guid("8C2F4E8E-7C1C-4C5B-8E2D-9F3A7D1B2C01"),
                        Email = "swapnil.bawankar@example.com",
                        EmailStatus = ContactStatus.Verified,
                        PhoneE164 = "+919876543210",
                        PhoneStatus = ContactStatus.Verified,
                        PendingEmail = (string?)null,
                        PendingEmailRequestedAt = (DateTimeOffset?)null,
                        PendingPhoneE164 = (string?)null,
                        PendingPhoneRequestedAt = (DateTimeOffset?)null,
                        LastEmailVerifiedAt = now.AddDays(-7),
                        LastPhoneVerifiedAt = now.AddDays(-7)
                    },
                    new
                    {
                        customer_id = new Guid("2A4B6C8D-1E2F-4A5B-9C0D-1E2F3A4B5C02"),
                        Email = "anita.deshmukh@example.com",
                        EmailStatus = ContactStatus.Verified,
                        PhoneE164 = "+919822011122",
                        PhoneStatus = ContactStatus.PendingVerification,
                        PendingEmail = (string?)null,
                        PendingEmailRequestedAt = (DateTimeOffset?)null,
                        PendingPhoneE164 = "+919700000000",
                        PendingPhoneRequestedAt = now.AddDays(-1),
                        LastEmailVerifiedAt = now.AddDays(-3),
                        LastPhoneVerifiedAt = now.AddDays(-30)
                    },
                    new
                    {
                        customer_id = new Guid("3B5C7D9E-2F3A-4B5C-8D9E-0F1A2B3C4D03"),
                        Email = "rahul.kulkarni@example.com",
                        EmailStatus = ContactStatus.PendingVerification,
                        PhoneE164 = "+919811122233",
                        PhoneStatus = ContactStatus.Verified,
                        PendingEmail = "rahul.k@example.com",
                        PendingEmailRequestedAt = now.AddHours(-12),
                        PendingPhoneE164 = (string?)null,
                        PendingPhoneRequestedAt = (DateTimeOffset?)null,
                        LastEmailVerifiedAt = now.AddDays(-90),
                        LastPhoneVerifiedAt = now.AddDays(-2)
                    }
                );
            });

            // ----- Preferences (owned, separate table) -----
            b.OwnsOne(p => p.Preferences, owned =>
            {
                owned.ToTable("preferences");
                owned.WithOwner().HasForeignKey("customer_id");
                owned.HasKey("customer_id");

                owned.Property(x => x.SmsEnabled).IsRequired();
                owned.Property(x => x.EmailEnabled).IsRequired();
                owned.Property(x => x.PushEnabled).IsRequired();
                owned.Property(x => x.RegulatoryConsentGiven).IsRequired();
                owned.Property(x => x.Language).HasMaxLength(16);
                owned.Property(x => x.TimeZone).HasMaxLength(64);
                owned.Property(x => x.UpdatedAt).IsRequired();

                var now = new DateTimeOffset(2026, 02, 01, 0, 0, 0, TimeSpan.Zero);

                owned.HasData(
                    new
                    {
                        customer_id = new Guid("8C2F4E8E-7C1C-4C5B-8E2D-9F3A7D1B2C01"),
                        SmsEnabled = true,
                        EmailEnabled = true,
                        PushEnabled = true,
                        RegulatoryConsentGiven = false,
                        Language = "en-IN",
                        TimeZone = "Asia/Kolkata",
                        UpdatedAt = now
                    },
                    new
                    {
                        customer_id = new Guid("2A4B6C8D-1E2F-4A5B-9C0D-1E2F3A4B5C02"),
                        SmsEnabled = true,
                        EmailEnabled = true,
                        PushEnabled = false,
                        RegulatoryConsentGiven = true,
                        Language = "en-IN",
                        TimeZone = "Asia/Kolkata",
                        UpdatedAt = now
                    },
                    new
                    {
                        customer_id = new Guid("3B5C7D9E-2F3A-4B5C-8D9E-0F1A2B3C4D03"),
                        SmsEnabled = false,
                        EmailEnabled = true,
                        PushEnabled = true,
                        RegulatoryConsentGiven = true,
                        Language = "en-IN",
                        TimeZone = "Asia/Kolkata",
                        UpdatedAt = now
                    }
                );
            });

            // ----- IMPORTANT: Seed owner using anonymous objects (no navigations) -----
            var created = new DateTimeOffset(2026, 02, 01, 0, 0, 0, TimeSpan.Zero);

            b.HasData(
                // Use anonymous types so 'Contact'/'Preferences' navigations are NOT constructed
                new { CustomerId = new Guid("8C2F4E8E-7C1C-4C5B-8E2D-9F3A7D1B2C01"), CreatedAt = created, UpdatedAt = created },
                new { CustomerId = new Guid("2A4B6C8D-1E2F-4A5B-9C0D-1E2F3A4B5C02"), CreatedAt = created, UpdatedAt = created },
                new { CustomerId = new Guid("3B5C7D9E-2F3A-4B5C-8D9E-0F1A2B3C4D03"), CreatedAt = created, UpdatedAt = created }
            );
        }
    }
}