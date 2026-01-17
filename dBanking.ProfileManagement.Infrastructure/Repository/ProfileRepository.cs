using System;
using System.Threading;
using System.Threading.Tasks;
using dBanking.ProfileManagement.Core.Entities;
using dBanking.ProfileManagement.Core.RepositoryContracts;
using dBanking.ProfileManagement.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace dBanking.ProfileManagement.Infrastructure.Repository
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ProfileDbContext _db;

        public ProfileRepository(ProfileDbContext db) => _db = db;

        public async Task<Profile?> GetAsync(Guid customerId, CancellationToken ct) =>
            await _db.Profiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.CustomerId == customerId, ct);

        public async Task AddAsync(Profile profile, CancellationToken ct)
        {
            await _db.Profiles.AddAsync(profile, ct);
        }

        public Task UpdateAsync(Profile profile, CancellationToken ct)
        {
            _db.Profiles.Update(profile);
            return Task.CompletedTask;
        }

        public async Task<bool> IsEmailInUseAsync(string email, Guid? excludeCustomerId, CancellationToken ct)
        {
            // check owned property
            var query = _db.Profiles.AsQueryable();
            if (excludeCustomerId.HasValue)
                query = query.Where(p => p.CustomerId != excludeCustomerId.Value);

            return await query.AnyAsync(p => p.Contact.Email == email, ct);
        }

        public async Task<bool> IsPhoneInUseAsync(string phoneE164, Guid? excludeCustomerId, CancellationToken ct)
        {
            var query = _db.Profiles.AsQueryable();
            if (excludeCustomerId.HasValue)
                query = query.Where(p => p.CustomerId != excludeCustomerId.Value);

            return await query.AnyAsync(p => p.Contact.PhoneE164 == phoneE164, ct);
        }
    }
}