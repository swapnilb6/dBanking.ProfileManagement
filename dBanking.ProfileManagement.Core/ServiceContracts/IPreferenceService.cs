namespace dBanking.ProfileManagement.Core.ServiceContracts
{
    using dBanking.ProfileManagement.Core.DTOs;
    using dBanking.ProfileManagement.Core.Entities;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPreferenceService
    {
        Task<PreferencesDto> GetAsync(Guid customerId, CancellationToken ct);

        Task<PreferencesDto> UpdateAsync(UpdatePreferencesRequestDto request, ActorContext actor, CancellationToken ct);
    }
}