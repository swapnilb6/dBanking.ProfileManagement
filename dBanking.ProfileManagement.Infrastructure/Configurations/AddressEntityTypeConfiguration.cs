using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dBanking.ProfileManagement.Infrastructure.DbContext.Configurations
{
    public class AddressEntityTypeConfiguration : IEntityTypeConfiguration<Address>
    {

        public void Configure(EntityTypeBuilder<Address> b)
        {
            b.ToTable("addresses");
            b.HasKey(a => a.AddressId);

            b.Property(a => a.AddressType).HasConversion<int>();
            b.Property(a => a.Line1).HasMaxLength(200).IsRequired();
            b.Property(a => a.Line2).HasMaxLength(200);
            b.Property(a => a.Line3).HasMaxLength(200);
            b.Property(a => a.City).HasMaxLength(100).IsRequired();
            b.Property(a => a.StateProvince).HasMaxLength(100).IsRequired();
            b.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
            b.Property(a => a.CountryCode).HasMaxLength(2).IsRequired();

            b.Property(a => a.IsPrimary).IsRequired();
            b.Property(a => a.EffectiveFrom).IsRequired();
            b.Property(a => a.CreatedAt).IsRequired();
            b.Property(a => a.UpdatedAt).IsRequired();

            b.HasIndex(a => a.CustomerId);

            // PostgreSQL partial unique index (enforce only one primary address per customer)
            b.HasIndex(a => a.CustomerId)
             .IsUnique()
             .HasDatabaseName("ux_addresses_one_primary_per_customer")
             .HasFilter("\"is_primary\" = true"); // Postgres WHERE clause

            b.HasOne<Profile>()
             .WithMany(p => p.Addresses)
             .HasForeignKey(a => a.CustomerId)
             .OnDelete(DeleteBehavior.Cascade);

            // Seed (unchanged) ...
            var c1 = new Guid("8C2F4E8E-7C1C-4C5B-8E2D-9F3A7D1B2C01");
            var c2 = new Guid("2A4B6C8D-1E2F-4A5B-9C0D-1E2F3A4B5C02");
            var c3 = new Guid("3B5C7D9E-2F3A-4B5C-8D9E-0F1A2B3C4D03");
            var now = new DateTimeOffset(2026, 02, 01, 0, 0, 0, TimeSpan.Zero);

            b.HasData(
                new Address
                {
                    AddressId = new Guid("11111111-1111-1111-1111-111111111111"),
                    CustomerId = c1,
                    AddressType = AddressType.Residential,
                    Line1 = "Flat 502, Sky Heights",
                    Line2 = "Baner",
                    City = "Pune",
                    StateProvince = "Maharashtra",
                    PostalCode = "411045",
                    CountryCode = "IN",
                    IsPrimary = true,
                    EffectiveFrom = now.AddYears(-1),
                    EffectiveTo = null,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Address
                {
                    AddressId = new Guid("22222222-2222-2222-2222-222222222222"),
                    CustomerId = c1,
                    AddressType = AddressType.Mailing,
                    Line1 = "Plot 10, Central Park",
                    Line2 = "Hinjawadi Phase 2",
                    City = "Pune",
                    StateProvince = "Maharashtra",
                    PostalCode = "411057",
                    CountryCode = "IN",
                    IsPrimary = false,
                    EffectiveFrom = now.AddMonths(-6),
                    EffectiveTo = null,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Address
                {
                    AddressId = new Guid("33333333-3333-3333-3333-333333333333"),
                    CustomerId = c2,
                    AddressType = AddressType.Residential,
                    Line1 = "601, Seaview Residency",
                    Line2 = "Worli",
                    City = "Mumbai",
                    StateProvince = "Maharashtra",
                    PostalCode = "400018",
                    CountryCode = "IN",
                    IsPrimary = true,
                    EffectiveFrom = now.AddYears(-2),
                    EffectiveTo = null,
                    CreatedAt = now,
                    UpdatedAt = now
                },
                new Address
                {
                    AddressId = new Guid("44444444-4444-4444-4444-444444444444"),
                    CustomerId = c3,
                    AddressType = AddressType.Work,
                    Line1 = "12, Green Meadows",
                    Line2 = "Kothrud",
                    City = "Pune",
                    StateProvince = "Maharashtra",
                    PostalCode = "411038",
                    CountryCode = "IN",
                    IsPrimary = true,
                    EffectiveFrom = now.AddMonths(-3),
                    EffectiveTo = null,
                    CreatedAt = now,
                    UpdatedAt = now
                }
            );
        }
    }
}