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

    public sealed class ContactService : IContactService
    {
        private readonly IProfileRepository _profiles;
        private readonly IVerificationTokenRepository _tokens;
        private readonly IAuditRepository _audits;
        private readonly IUnitOfWork _uow;
        private readonly IProfileEventPublisher _events;
        private readonly IVerificationService _verification;
        private readonly IClock _clock;
        private readonly IMapper _mapper;

        public ContactService(
            IProfileRepository profiles,
            IVerificationTokenRepository tokens,
            IAuditRepository audits,
            IUnitOfWork uow,
            IProfileEventPublisher eventsPublisher,
            IVerificationService verification,
            IClock clock,
            IMapper mapper)
        {
            _profiles = profiles;
            _tokens = tokens;
            _audits = audits;
            _uow = uow;
            _events = eventsPublisher;
            _verification = verification;
            _clock = clock;
            _mapper = mapper;
        }

        public async Task<ContactViewDto> GetAsync(Guid customerId, CancellationToken ct)
        {
            // Load profile; map to ContactViewDto
            // Throw NotFound if profile missing
            throw new NotImplementedException();
        }

        public async Task<ContactChangeResultDto> RequestEmailChangeAsync(
            ChangeEmailRequestDto request, ActorContext actor, CancellationToken ct)
        {
            // Steps:
            // 1) Normalize email (lowercase), check uniqueness via _profiles.IsEmailInUseAsync
            // 2) Load or create Profile; set Contact.PendingEmail + timestamps
            // 3) Create verification token using _verification.CreateAsync(type: "EmailLink")
            // 4) Persist changes + audit (old/new snapshots)
            // 5) Publish ProfileEmailChangeRequested event
            // 6) Return ContactChangeResultDto { Success=true, Status="PendingVerification" }
            throw new NotImplementedException();
        }

        public async Task<OperationResultDto> VerifyEmailAsync(
            VerifyEmailRequestDto request, ActorContext actor, CancellationToken ct)
        {
            // Steps:
            // 1) Validate token with _verification.VerifyAsync
            // 2) On success: move PendingEmail -> Email, set status Verified, clear pending; timestamps
            // 3) Persist + audit; publish ProfileEmailVerified; notify old & new channels (downstream)
            // 4) Return OperationResultDto { Success=true, Code="OK" }
            throw new NotImplementedException();
        }

        public async Task<ContactChangeResultDto> RequestPhoneChangeAsync(
            ChangePhoneRequestDto request, ActorContext actor, CancellationToken ct)
        {
            // Steps:
            // 1) Normalize E.164; check uniqueness
            // 2) Set PendingPhoneE164; create OTP verification
            // 3) Persist + audit; publish change-requested (optional)
            // 4) Return PendingVerification
            throw new NotImplementedException();
        }

        public async Task<OperationResultDto> VerifyPhoneAsync(
            VerifyPhoneRequestDto request, ActorContext actor, CancellationToken ct)
        {
            // Steps mirror email verification; commit phone; publish ProfilePhoneVerified
            throw new NotImplementedException();
        }
    }
}
