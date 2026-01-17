using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.DbContext.Configurations
{
    public class AddressEntityTypeConfiguration : IEntityTypeConfiguration<Address>
    {
        public void Configure(EntityTypeBuilder<Address> builder)
        {
            builder.ToTable("address");
            builder.HasKey(a => a.AddressId);

            builder.Property(a => a.CustomerId).IsRequired();

            builder.Property(a => a.AddressType)
                .HasConversion<int>()
                .HasColumnName("address_type")
                .IsRequired();

            builder.Property(a => a.Line1).HasMaxLength(200).IsRequired();
            builder.Property(a => a.Line2).HasMaxLength(200);
            builder.Property(a => a.Line3).HasMaxLength(200);
            builder.Property(a => a.City).HasMaxLength(100).IsRequired();
            builder.Property(a => a.StateProvince).HasMaxLength(100).IsRequired();
            builder.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
            builder.Property(a => a.CountryCode).HasMaxLength(2).IsRequired();
            builder.Property(a => a.IsPrimary).HasColumnName("is_primary").IsRequired();
            builder.Property(a => a.EffectiveFrom).IsRequired();
            builder.Property(a => a.EffectiveTo);

            builder.Property(a => a.CreatedAt).IsRequired();
            builder.Property(a => a.UpdatedAt).IsRequired();

            builder.HasIndex(a => a.CustomerId).HasDatabaseName("ix_address_customer");
            builder.HasIndex(a => new { a.CustomerId, a.AddressType }).HasDatabaseName("ix_address_cust_type");

            // Guarantee only one primary address per customer
            builder.HasIndex(a => a.CustomerId)
                .IsUnique()
                .HasFilter("is_primary = true")
                .HasDatabaseName("ux_address_primary_per_customer");

            // Enum check (optional)
            builder.ToTable(t =>
            {
                t.HasCheckConstraint("chk_address_type", "address_type IN (0,1,2)");
            });

            // FK optional (if you want to enforce referential integrity to profile)
            builder.HasOne<Profile>()
                .WithMany(p => p.Addresses)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}