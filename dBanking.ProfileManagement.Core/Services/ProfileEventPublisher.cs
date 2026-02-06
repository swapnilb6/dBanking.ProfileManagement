
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using dBanking.ProfileManagement.Core.ServiceContracts;
using dBanking.ProfileManagement.Core.Events;

namespace dBanking.ProfileManagement.Infrastructure
{
    public sealed class ProfileEventPublisher : IProfileEventPublisher
    {
        private readonly ILogger<ProfileEventPublisher> _logger;

        public ProfileEventPublisher(ILogger<ProfileEventPublisher> logger)
        {
            _logger = logger;
        }

        public Task PublishAsync(ProfileEmailChangeRequested evt, CancellationToken ct)
        {
            _logger.LogInformation("Publish ProfileEmailChangeRequested: {CustomerId} CorrelationId:{CorrelationId} RequestedAt:{RequestedAt}",
                evt.CustomerId, evt.CorrelationId, evt.RequestedAt);
            return Task.CompletedTask;
        }

        public Task PublishAsync(ProfileEmailVerified evt, CancellationToken ct)
        {
            _logger.LogInformation("Publish ProfileEmailVerified: {CustomerId} CorrelationId:{CorrelationId} VerifiedAt:{VerifiedAt}",
                evt.CustomerId, evt.CorrelationId, evt.VerifiedAt);
            return Task.CompletedTask;
        }

        public Task PublishAsync(ProfilePhoneVerified evt, CancellationToken ct)
        {
            _logger.LogInformation("Publish ProfilePhoneVerified: {CustomerId} CorrelationId:{CorrelationId} VerifiedAt:{VerifiedAt}",
                evt.CustomerId, evt.CorrelationId, evt.VerifiedAt);
            return Task.CompletedTask;
        }

        public Task PublishAsync(ProfileAddressUpdated evt, CancellationToken ct)
        {
            _logger.LogInformation("Publish ProfileAddressUpdated: {CustomerId} AddressId:{AddressId} CorrelationId:{CorrelationId} EffectiveFrom:{EffectiveFrom}",
                evt.CustomerId, evt.AddressId, evt.CorrelationId, evt.EffectiveFrom);
            return Task.CompletedTask;
        }

        public Task PublishAsync(ProfilePreferencesUpdated evt, CancellationToken ct)
        {
            _logger.LogInformation("Publish ProfilePreferencesUpdated: {CustomerId} CorrelationId:{CorrelationId} UpdatedAt:{UpdatedAt}",
                evt.CustomerId, evt.CorrelationId, evt.UpdatedAt);
            return Task.CompletedTask;
        }
    }
}