namespace dBanking.ProfileManagement.Core.ServiceContracts
{
    using dBanking.ProfileManagement.Core.Events;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IProfileEventPublisher
    {
        Task PublishAsync(ProfileEmailChangeRequested evt, CancellationToken ct);
        Task PublishAsync(ProfileEmailVerified evt, CancellationToken ct);
        Task PublishAsync(ProfilePhoneVerified evt, CancellationToken ct);
        Task PublishAsync(ProfileAddressUpdated evt, CancellationToken ct);
        Task PublishAsync(ProfilePreferencesUpdated evt, CancellationToken ct);
    }
}
