namespace dBanking.ProfileManagement.Core.ServiceContracts
{
    using dBanking.ProfileManagement.Core.DTOs;
    using dBanking.ProfileManagement.Core.Entities;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IAddressService
    {
        Task<IReadOnlyList<AddressDto>> GetByCustomerAsync(Guid customerId, CancellationToken ct);

        Task<AddressDto> UpsertAsync(UpsertAddressRequestDto request, ActorContext actor, CancellationToken ct);

        Task<AddressDto> UpdateAsync(UpdateAddressRequestDto request, ActorContext actor, CancellationToken ct);

        Task<OperationResultDto> SetPrimaryAsync(Guid customerId, Guid addressId, ActorContext actor, CancellationToken ct);
    }
}
