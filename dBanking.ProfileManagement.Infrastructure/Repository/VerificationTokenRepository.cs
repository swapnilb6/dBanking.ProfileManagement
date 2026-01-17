using System;
using System.Threading;
using System.Threading.Tasks;
using dBanking.ProfileManagement.Core.Entities;
using dBanking.ProfileManagement.Core.RepositoryContracts;
using dBanking.ProfileManagement.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace dBanking.ProfileManagement.Infrastructure.Repository
{
    public class VerificationTokenRepository : IVerificationTokenRepository
    {
        private readonly ProfileDbContext _db;

        public VerificationTokenRepository(ProfileDbContext db) => _db = db;

        public async Task AddAsync(VerificationToken token, CancellationToken ct)
        {
            await _db.VerificationTokens.AddAsync(token, ct);
        }

        public async Task<VerificationToken?> GetPendingAsync(Guid customerId, VerificationType type, CancellationToken ct)
        {
            return await _db.VerificationTokens
                .FirstOrDefaultAsync(v => v.CustomerId == customerId
                                       && v.Type == type
                                       && v.Status == VerificationStatus.Pending, ct);
        }

        public async Task<VerificationToken?> GetByIdAsync(Guid verificationId, CancellationToken ct)
        {
            return await _db.VerificationTokens
                .FirstOrDefaultAsync(v => v.VerificationId == verificationId, ct);
        }

        public Task UpdateAsync(VerificationToken token, CancellationToken ct)
        {
            _db.VerificationTokens.Update(token);
            return Task.CompletedTask;
        }
    }
}