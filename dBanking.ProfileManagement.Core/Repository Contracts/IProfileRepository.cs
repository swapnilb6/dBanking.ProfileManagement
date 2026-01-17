namespace dBanking.ProfileManagement.Core.RepositoryContracts
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using dBanking.ProfileManagement.Core.Entities;

    public interface IProfileRepository
    {
        Task<Profile?> GetAsync(Guid customerId, CancellationToken ct);
        Task AddAsync(Profile profile, CancellationToken ct);
        Task UpdateAsync(Profile profile, CancellationToken ct);

        // Optional uniqueness checks
        Task<bool> IsEmailInUseAsync(string email, Guid? excludeCustomerId, CancellationToken ct);
        Task<bool> IsPhoneInUseAsync(string phoneE164, Guid? excludeCustomerId, CancellationToken ct);
    }
}
