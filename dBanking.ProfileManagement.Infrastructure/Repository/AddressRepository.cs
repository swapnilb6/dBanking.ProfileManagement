
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dBanking.ProfileManagement.Core.Entities;
using dBanking.ProfileManagement.Core.RepositoryContracts;
using dBanking.ProfileManagement.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace dBanking.ProfileManagement.Infrastructure.Repository
{
    public class AddressRepository : IAddressRepository
    {
        private readonly ProfileDbContext _db;

        public AddressRepository(ProfileDbContext db) => _db = db;

        public async Task<IReadOnlyList<Address>> GetByCustomerAsync(Guid customerId, CancellationToken ct) =>
            await _db.Addresses
                     .AsNoTracking()
                     .Where(a => a.CustomerId == customerId)
                     .OrderByDescending(a => a.IsPrimary)
                     .ThenByDescending(a => a.UpdatedAt)
                     .ToListAsync(ct);

        public async Task<Address?> GetAsync(Guid customerId, Guid addressId, CancellationToken ct) =>
            await _db.Addresses
                     .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.AddressId == addressId, ct);

        public async Task UpsertAsync(Address address, CancellationToken ct)
        {
            var exists = await _db.Addresses.AnyAsync(a => a.AddressId == address.AddressId, ct);
            if (exists) _db.Addresses.Update(address);
            else await _db.Addresses.AddAsync(address, ct);
        }

        public Task UpdateAsync(Address address, CancellationToken ct)
        {
            _db.Addresses.Update(address);
            return Task.CompletedTask;
        }

        public async Task SetPrimaryAsync(Guid customerId, Guid addressId, CancellationToken ct)
        {
            // Clear previous primary
            var currentPrimaries = await _db.Addresses
                .Where(a => a.CustomerId == customerId && a.IsPrimary)
                .ToListAsync(ct);

            foreach (var a in currentPrimaries)
                a.IsPrimary = false;

            // Set the requested one
            var target = await _db.Addresses
                .FirstOrDefaultAsync(a => a.CustomerId == customerId && a.AddressId == addressId, ct)
                ?? throw new InvalidOperationException("Address not found.");

            target.IsPrimary = true;
        }
    }
}