using dBanking.ProfileManagement.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace dBanking.ProfileManagement.Infrastructure.DbContext
{
    public class ProfileDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public ProfileDbContext(DbContextOptions<ProfileDbContext> options) : base(options) { }

        public DbSet<Profile> Profiles => Set<Profile>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<VerificationToken> VerificationTokens => Set<VerificationToken>();
        public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all entity type configurations from this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProfileDbContext).Assembly);

            // Global snake_case naming is set in options (UseSnakeCaseNamingConvention)
            base.OnModelCreating(modelBuilder);
        }
    }
}
