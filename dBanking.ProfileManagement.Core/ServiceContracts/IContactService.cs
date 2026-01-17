namespace dBanking.ProfileManagement.Core.ServiceContracts
{
    using dBanking.ProfileManagement.Core.DTOs;
    using dBanking.ProfileManagement.Core.Entities;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IContactService
    {
        Task<ContactViewDto> GetAsync(Guid customerId, CancellationToken ct);

        Task<ContactChangeResultDto> RequestEmailChangeAsync(
            ChangeEmailRequestDto request, ActorContext actor, CancellationToken ct);

        Task<OperationResultDto> VerifyEmailAsync(
            VerifyEmailRequestDto request, ActorContext actor, CancellationToken ct);

        Task<ContactChangeResultDto> RequestPhoneChangeAsync(
            ChangePhoneRequestDto request, ActorContext actor, CancellationToken ct);

        Task<OperationResultDto> VerifyPhoneAsync(
            VerifyPhoneRequestDto request, ActorContext actor, CancellationToken ct);
    }
}
