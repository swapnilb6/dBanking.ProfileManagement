
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
            var profile = await _profiles.GetAsync(customerId, ct)
                          ?? throw new KeyNotFoundException("Profile not found.");

            return _mapper.Map<ContactViewDto>(profile);
        }

        public async Task<ContactChangeResultDto> RequestEmailChangeAsync(
            ChangeEmailRequestDto request, ActorContext actor, CancellationToken ct)
        {
            var newEmail = request.NewEmail.Trim().ToLowerInvariant();

            // Load or create profile
            var profile = await _profiles.GetAsync(request.CustomerId, ct);
            if (profile is null)
            {
                profile = new Entities.Profile
                {
                    CustomerId = request.CustomerId,
                    CreatedAt = _clock.UtcNow,
                    UpdatedAt = _clock.UtcNow,
                    Contact = new Contact { EmailStatus = ContactStatus.Verified, PhoneStatus = ContactStatus.Verified }
                };
                await _profiles.AddAsync(profile, ct);
            }

            // Idempotent short-circuit: if pending already same email, return pending
            if (string.Equals(profile.Contact.PendingEmail, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new ContactChangeResultDto
                {
                    Success = true,
                    Status = "PendingVerification",
                    Message = "Verification already initiated for this email."
                };
            }

            // If email is already current, nothing to do
            if (string.Equals(profile.Contact.Email, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                return new ContactChangeResultDto
                {
                    Success = true,
                    Status = "Verified",
                    Message = "Email already verified and active."
                };
            }

            // Uniqueness check (if global uniqueness is required)
            var inUse = await _profiles.IsEmailInUseAsync(newEmail, excludeCustomerId: request.CustomerId, ct);
            if (inUse) throw new InvalidOperationException("Email is already associated with another profile.");

            // Snapshot for audit
            var oldSnapshot = new
            {
                profile.Contact.Email,
                profile.Contact.EmailStatus,
                profile.Contact.PendingEmail,
                profile.Contact.PendingEmailRequestedAt
            };

            // Set pending and create token
            profile.Contact.PendingEmail = newEmail;
            profile.Contact.PendingEmailRequestedAt = _clock.UtcNow;
            profile.UpdatedAt = _clock.UtcNow;

            var (verificationId, rawToken) = await _verification.CreateAsync(
                customerId: request.CustomerId,
                channelValue: newEmail,
                type: "EmailLink",
                correlationId: actor.CorrelationId,
                ttlMinutes: 30,
                ct);

            // Audit
            var newSnapshot = new
            {
                profile.Contact.Email,
                profile.Contact.EmailStatus,
                profile.Contact.PendingEmail,
                profile.Contact.PendingEmailRequestedAt
            };

            await _audits.AddAsync(new AuditRecord
            {
                CustomerId = request.CustomerId,
                Entity = "Contacts",
                EntityId = "Email",
                Operation = "ChangeRequested",
                OldValueJson = JsonHelper.ToJson(oldSnapshot),
                NewValueJson = JsonHelper.ToJson(newSnapshot),
                ChangedFieldsCsv = "PendingEmail,PendingEmailRequestedAt",
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

            // Publish event (masked)
            await _events.PublishAsync(new ProfileEmailChangeRequested(
                CustomerId: request.CustomerId,
                NewEmailMasked: Masking.MaskEmail(newEmail),
                CorrelationId: actor.CorrelationId ?? string.Empty,
                RequestedAt: _clock.UtcNow), ct);

            // The rawToken is intended for Notification service (email link),
            // which will be triggered by the consumer of the event in infra.

            return new ContactChangeResultDto
            {
                Success = true,
                Status = "PendingVerification",
                Message = "Verification link sent to the new email."
            };
        }

        public async Task<OperationResultDto> VerifyEmailAsync(
            VerifyEmailRequestDto request, ActorContext actor, CancellationToken ct)
        {
            var profile = await _profiles.GetAsync(request.CustomerId, ct)
                          ?? throw new KeyNotFoundException("Profile not found.");

            var pending = profile.Contact.PendingEmail;
            if (string.IsNullOrWhiteSpace(pending))
                throw new InvalidOperationException("No pending email change found.");

            // Validate token (VerificationService handles lookup & status)
            var ok = await _verification.VerifyAsync(request.CustomerId, /*verificationId unknown*/ Guid.Empty, request.VerificationToken, ct);
            if (!ok) throw new InvalidOperationException("Invalid or expired token.");

            // Snapshot for audit
            var oldSnapshot = new
            {
                profile.Contact.Email,
                profile.Contact.EmailStatus,
                profile.Contact.PendingEmail
            };

            // Commit the change
            var previousEmail = profile.Contact.Email;
            profile.Contact.Email = pending;
            profile.Contact.EmailStatus = ContactStatus.Verified;
            profile.Contact.PendingEmail = null;
            profile.Contact.PendingEmailRequestedAt = null;
            profile.Contact.LastEmailVerifiedAt = _clock.UtcNow;
            profile.UpdatedAt = _clock.UtcNow;

            await _audits.AddAsync(new AuditRecord
            {
                CustomerId = request.CustomerId,
                Entity = "Contacts",
                EntityId = "Email",
                Operation = "Verified",
                OldValueJson = JsonHelper.ToJson(oldSnapshot),
                NewValueJson = JsonHelper.ToJson(new { profile.Contact.Email, profile.Contact.EmailStatus }),
                ChangedFieldsCsv = "Email,EmailStatus,PendingEmail,PendingEmailRequestedAt,LastEmailVerifiedAt",
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

            await _events.PublishAsync(new ProfileEmailVerified(
                CustomerId: request.CustomerId,
                EmailMasked: Masking.MaskEmail(profile.Contact.Email ?? ""),
                CorrelationId: actor.CorrelationId ?? string.Empty,
                VerifiedAt: _clock.UtcNow), ct);

            // Notifications to old & new contact channels are done downstream (consumer side)

            return new OperationResultDto { Success = true, Code = "OK", Message = "Email verified and updated." };
        }

        public async Task<ContactChangeResultDto> RequestPhoneChangeAsync(
            ChangePhoneRequestDto request, ActorContext actor, CancellationToken ct)
        {
            var newPhone = request.NewPhoneE164.Trim();

            var profile = await _profiles.GetAsync(request.CustomerId, ct);
            if (profile is null)
            {
                profile = new Entities.Profile
                {
                    CustomerId = request.CustomerId,
                    CreatedAt = _clock.UtcNow,
                    UpdatedAt = _clock.UtcNow,
                    Contact = new Contact { EmailStatus = ContactStatus.Verified, PhoneStatus = ContactStatus.Verified }
                };
                await _profiles.AddAsync(profile, ct);
            }

            if (string.Equals(profile.Contact.PendingPhoneE164, newPhone, StringComparison.Ordinal))
            {
                return new ContactChangeResultDto
                {
                    Success = true,
                    Status = "PendingVerification",
                    Message = "Verification already initiated for this phone."
                };
            }

            if (string.Equals(profile.Contact.PhoneE164, newPhone, StringComparison.Ordinal))
            {
                return new ContactChangeResultDto
                {
                    Success = true,
                    Status = "Verified",
                    Message = "Phone already verified and active."
                };
            }

            var inUse = await _profiles.IsPhoneInUseAsync(newPhone, excludeCustomerId: request.CustomerId, ct);
            if (inUse) throw new InvalidOperationException("Phone number is already associated with another profile.");

            var oldSnapshot = new
            {
                profile.Contact.PhoneE164,
                profile.Contact.PhoneStatus,
                profile.Contact.PendingPhoneE164,
                profile.Contact.PendingPhoneRequestedAt
            };

            profile.Contact.PendingPhoneE164 = newPhone;
            profile.Contact.PendingPhoneRequestedAt = _clock.UtcNow;
            profile.UpdatedAt = _clock.UtcNow;

            var (_, rawOtp) = await _verification.CreateAsync(
                customerId: request.CustomerId,
                channelValue: newPhone,
                type: "SmsOtp",
                correlationId: actor.CorrelationId,
                ttlMinutes: 10,
                ct);

            var newSnapshot = new
            {
                profile.Contact.PhoneE164,
                profile.Contact.PhoneStatus,
                profile.Contact.PendingPhoneE164,
                profile.Contact.PendingPhoneRequestedAt
            };

            await _audits.AddAsync(new AuditRecord
            {
                CustomerId = request.CustomerId,
                Entity = "Contacts",
                EntityId = "Phone",
                Operation = "ChangeRequested",
                OldValueJson = JsonHelper.ToJson(oldSnapshot),
                NewValueJson = JsonHelper.ToJson(newSnapshot),
                ChangedFieldsCsv = "PendingPhoneE164,PendingPhoneRequestedAt",
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

            // Optionally publish a "change requested" event for phone (kept symmetric with email)
            // ... could be added later if required

            return new ContactChangeResultDto
            {
                Success = true,
                Status = "PendingVerification",
                Message = "OTP sent to the new phone."
            };
        }

        public async Task<OperationResultDto> VerifyPhoneAsync(
            VerifyPhoneRequestDto request, ActorContext actor, CancellationToken ct)
        {
            var profile = await _profiles.GetAsync(request.CustomerId, ct)
                          ?? throw new KeyNotFoundException("Profile not found.");

            var pending = profile.Contact.PendingPhoneE164;
            if (string.IsNullOrWhiteSpace(pending))
                throw new InvalidOperationException("No pending phone change found.");

            var ok = await _verification.VerifyAsync(request.CustomerId, /*verificationId unknown*/ Guid.Empty, request.OtpCode, ct);
            if (!ok) throw new InvalidOperationException("Invalid or expired OTP.");

            var oldSnapshot = new
            {
                profile.Contact.PhoneE164,
                profile.Contact.PhoneStatus,
                profile.Contact.PendingPhoneE164
            };

            profile.Contact.PhoneE164 = pending;
            profile.Contact.PhoneStatus = ContactStatus.Verified;
            profile.Contact.PendingPhoneE164 = null;
            profile.Contact.PendingPhoneRequestedAt = null;
            profile.Contact.LastPhoneVerifiedAt = _clock.UtcNow;
            profile.UpdatedAt = _clock.UtcNow;

            await _audits.AddAsync(new AuditRecord
            {
                CustomerId = request.CustomerId,
                Entity = "Contacts",
                EntityId = "Phone",
                Operation = "Verified",
                OldValueJson = JsonHelper.ToJson(oldSnapshot),
                NewValueJson = JsonHelper.ToJson(new { profile.Contact.PhoneE164, profile.Contact.PhoneStatus }),
                ChangedFieldsCsv = "PhoneE164,PhoneStatus,PendingPhoneE164,PendingPhoneRequestedAt,LastPhoneVerifiedAt",
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

            await _events.PublishAsync(new ProfilePhoneVerified(
                CustomerId: request.CustomerId,
                PhoneMasked: Masking.MaskPhone(profile.Contact.PhoneE164 ?? ""),
                CorrelationId: actor.CorrelationId ?? string.Empty,
                VerifiedAt: _clock.UtcNow), ct);

            return new OperationResultDto { Success = true, Code = "OK", Message = "Phone verified and updated." };
        }
    }
}