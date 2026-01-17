using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.Configurations
{
    public class ProfileEntityTypeConfiguration : IEntityTypeConfiguration<Profile>
    {
        public void Configure(EntityTypeBuilder<Profile> builder)
        {
            builder.ToTable("profile");
            builder.HasKey(p => p.CustomerId);

            builder.Property(p => p.CreatedAt).IsRequired();
            builder.Property(p => p.UpdatedAt).IsRequired();

            // We rely on xmin concurrency (PostgreSQL). Ignore RowVersion property.
            builder.Ignore(p => p.RowVersion);

            // Use xmin as concurrency token
            builder.UseXminAsConcurrencyToken();

            // ----- Contact (owned)
            builder.OwnsOne(p => p.Contact, contact =>
            {
                contact.Property(c => c.Email)
                    .HasColumnName("email")
                    .HasMaxLength(254);

                contact.Property(c => c.EmailStatus)
                    .HasColumnName("email_status")
                    .HasConversion<int>()
                    .IsRequired();

                contact.Property(c => c.PhoneE164)
                    .HasColumnName("phone_e164")
                    .HasMaxLength(20);

                contact.Property(c => c.PhoneStatus)
                    .HasColumnName("phone_status")
                    .HasConversion<int>()
                    .IsRequired();

                contact.Property(c => c.PendingEmail)
                    .HasColumnName("pending_email")
                    .HasMaxLength(254);

                contact.Property(c => c.PendingEmailRequestedAt)
                    .HasColumnName("pending_email_requested_at");

                contact.Property(c => c.PendingPhoneE164)
                    .HasColumnName("pending_phone_e164")
                    .HasMaxLength(20);

                contact.Property(c => c.PendingPhoneRequestedAt)
                    .HasColumnName("pending_phone_requested_at");

                contact.Property(c => c.LastEmailVerifiedAt)
                    .HasColumnName("last_email_verified_at");

                contact.Property(c => c.LastPhoneVerifiedAt)
                    .HasColumnName("last_phone_verified_at");

                // Unique partial index on Email (when not null)
                contact.HasIndex(c => c.Email)
                       .IsUnique()
                       .HasFilter("email IS NOT NULL")
                       .HasDatabaseName("ux_profile_email");

                // Unique partial index on Phone (when not null)
                contact.HasIndex(c => c.PhoneE164)
                       .IsUnique()
                       .HasFilter("phone_e164 IS NOT NULL")
                       .HasDatabaseName("ux_profile_phone");
            });

            // ----- Preferences (owned)
            builder.OwnsOne(p => p.Preferences, prefs =>
            {
                prefs.Property(x => x.SmsEnabled).HasColumnName("pref_sms_enabled");
                prefs.Property(x => x.EmailEnabled).HasColumnName("pref_email_enabled");
                prefs.Property(x => x.PushEnabled).HasColumnName("pref_push_enabled");
                prefs.Property(x => x.RegulatoryConsentGiven).HasColumnName("pref_reg_consent_given");
                prefs.Property(x => x.Language).HasColumnName("pref_language").HasMaxLength(10);
                prefs.Property(x => x.TimeZone).HasColumnName("pref_time_zone").HasMaxLength(64);
                prefs.Property(x => x.UpdatedAt).HasColumnName("pref_updated_at");
            });
        }
    }
}
