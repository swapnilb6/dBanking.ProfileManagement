namespace dBanking.ProfileManagement.Core.RepositoryContracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using dBanking.ProfileManagement.Core.Entities;

    public interface IAddressRepository
    {
        Task<IReadOnlyList<Address>> GetByCustomerAsync(Guid customerId, CancellationToken ct);
        Task<Address?> GetAsync(Guid customerId, Guid addressId, CancellationToken ct);
        Task UpsertAsync(Address address, CancellationToken ct);
        Task UpdateAsync(Address address, CancellationToken ct);
        Task SetPrimaryAsync(Guid customerId, Guid addressId, CancellationToken ct);
    }
}
