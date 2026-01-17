namespace dBanking.ProfileManagement.Core.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AutoMapper;
    using dBanking.ProfileManagement.Core.DTOs;
    using dBanking.ProfileManagement.Core.Entities;
    using dBanking.ProfileManagement.Core.RepositoryContracts;
    using dBanking.ProfileManagement.Core.ServiceContracts;
    using dBanking.ProfileManagement.Core.Events;

    public sealed class PreferenceService : IPreferenceService
    {
        private readonly IProfileRepository _profiles;
        private readonly IAuditRepository _audits;
        private readonly IUnitOfWork _uow;
        private readonly IProfileEventPublisher _events;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public PreferenceService(
            IProfileRepository profiles,
            IAuditRepository audits,
            IUnitOfWork uow,
            IProfileEventPublisher eventsPublisher,
            IClock clock,
            IMapper mapper)
        {
            _profiles = profiles;
            _audits = audits;
            _uow = uow;
            _events = eventsPublisher;
            _clock = clock;
            _mapper = mapper;
        }

        public async Task<PreferencesDto> GetAsync(Guid customerId, CancellationToken ct)
        {
            // Load profile; return PreferencesDto
            throw new NotImplementedException();
        }

        public async Task<PreferencesDto> UpdateAsync(UpdatePreferencesRequestDto request, ActorContext actor, CancellationToken ct)
        {
            // Steps:
            // 1) Load profile; apply partial updates to profile.Preferences
            // 2) Persist + audit changed fields; publish ProfilePreferencesUpdated
            // 3) Return PreferencesDto
            throw new NotImplementedException();
        }
    }
}
