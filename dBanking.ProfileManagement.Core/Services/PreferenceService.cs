
using AutoMapper;
using dBanking.ProfileManagement.Core.DTOs;
using dBanking.ProfileManagement.Core.Entities;
using dBanking.ProfileManagement.Core.Events;
using dBanking.ProfileManagement.Core.RepositoryContracts;
using dBanking.ProfileManagement.Core.ServiceContracts;
using dBanking.ProfileManagement.Core.Services.Internals;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Services
{
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
            var profile = await _profiles.GetAsync(customerId, ct)
                          ?? throw new KeyNotFoundException("Profile not found.");

            return _mapper.Map<PreferencesDto>(profile.Preferences);
        }

        public async Task<PreferencesDto> UpdateAsync(UpdatePreferencesRequestDto request, ActorContext actor, CancellationToken ct)
        {
            var profile = await _profiles.GetAsync(request.CustomerId, ct);
            if (profile is null)
            {
                // initialize profile if not present
                profile = new Entities.Profile
                {
                    CustomerId = request.CustomerId,
                    CreatedAt = _clock.UtcNow,
                    UpdatedAt = _clock.UtcNow
                };
                await _profiles.AddAsync(profile, ct);
            }

            var before = new Preferences
            {
                SmsEnabled = profile.Preferences.SmsEnabled,
                EmailEnabled = profile.Preferences.EmailEnabled,
                PushEnabled = profile.Preferences.PushEnabled,
                RegulatoryConsentGiven = profile.Preferences.RegulatoryConsentGiven,
                Language = profile.Preferences.Language,
                TimeZone = profile.Preferences.TimeZone,
                UpdatedAt = profile.Preferences.UpdatedAt
            };

            // Apply partial updates (validator already ensures limits)
            if (request.SmsEnabled.HasValue) profile.Preferences.SmsEnabled = request.SmsEnabled.Value;
            if (request.EmailEnabled.HasValue) profile.Preferences.EmailEnabled = request.EmailEnabled.Value;
            if (request.PushEnabled.HasValue) profile.Preferences.PushEnabled = request.PushEnabled.Value;
            if (request.RegulatoryConsentGiven.HasValue) profile.Preferences.RegulatoryConsentGiven = request.RegulatoryConsentGiven.Value;
            if (!string.IsNullOrWhiteSpace(request.Language)) profile.Preferences.Language = request.Language;
            if (!string.IsNullOrWhiteSpace(request.TimeZone)) profile.Preferences.TimeZone = request.TimeZone;

            profile.Preferences.UpdatedAt = _clock.UtcNow;
            profile.UpdatedAt = _clock.UtcNow;

            var after = profile.Preferences;

            var changedCsv = DiffHelper.ChangedFieldsCsv(before, after);

            await _audits.AddAsync(new AuditRecord
            {
                CustomerId = request.CustomerId,
                Entity = "Preferences",
                EntityId = "Preferences",
                Operation = "Update",
                OldValueJson = JsonHelper.ToJson(before),
                NewValueJson = JsonHelper.ToJson(after),
                ChangedFieldsCsv = string.IsNullOrWhiteSpace(changedCsv) ? "NONE" : changedCsv,
                ActorId = actor.ActorId,
                ActorRole = actor.ActorRole,
                SourceChannel = actor.SourceChannel,
                ReasonCode = actor.Reason,
                IpAddress = actor.IpAddress,
                UserAgent = actor.UserAgent,
                CorrelationId = actor.CorrelationId,
                Timestamp = _clock.UtcNow
            }, ct);

            await _uow.SaveChangesAsync(ct);

            await _events.PublishAsync(new ProfilePreferencesUpdated(
                CustomerId: request.CustomerId,
                SmsEnabled: request.SmsEnabled,
                EmailEnabled: request.EmailEnabled,
                PushEnabled: request.PushEnabled,
                RegulatoryConsentGiven: request.RegulatoryConsentGiven,
                Language: request.Language,
                TimeZone: request.TimeZone,
                CorrelationId: actor.CorrelationId ?? string.Empty,
                UpdatedAt: profile.Preferences.UpdatedAt
            ), ct);

            return _mapper.Map<PreferencesDto>(profile.Preferences);
        }
    }
}