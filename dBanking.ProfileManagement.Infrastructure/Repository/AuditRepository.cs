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
    public class AuditRepository : IAuditRepository
    {
        private readonly ProfileDbContext _db;

        public AuditRepository(ProfileDbContext db) => _db = db;

        public async Task AddAsync(AuditRecord record, CancellationToken ct)
        {
            await _db.AuditRecords.AddAsync(record, ct);
        }

        public async Task<IReadOnlyList<AuditRecord>> GetAsync(Guid customerId, string? entityFilter, int skip, int take, CancellationToken ct)
        {
            var query = _db.AuditRecords.AsNoTracking()
                .Where(a => a.CustomerId == customerId);

            if (!string.IsNullOrWhiteSpace(entityFilter))
                query = query.Where(a => a.Entity == entityFilter);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Skip(skip)
                .Take(take)
                .ToListAsync(ct);
        }
    }
}
