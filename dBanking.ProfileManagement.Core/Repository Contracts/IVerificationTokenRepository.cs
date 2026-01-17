namespace dBanking.ProfileManagement.Core.RepositoryContracts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using dBanking.ProfileManagement.Core.Entities;

    public interface IVerificationTokenRepository
    {
        Task AddAsync(VerificationToken token, CancellationToken ct);
        Task<VerificationToken?> GetPendingAsync(Guid customerId, VerificationType type, CancellationToken ct);
        Task<VerificationToken?> GetByIdAsync(Guid verificationId, CancellationToken ct);
        Task UpdateAsync(VerificationToken token, CancellationToken ct);
    }
}
