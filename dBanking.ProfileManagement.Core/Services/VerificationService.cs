namespace dBanking.ProfileManagement.Core.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using dBanking.ProfileManagement.Core.RepositoryContracts;
    using dBanking.ProfileManagement.Core.ServiceContracts;
    using dBanking.ProfileManagement.Core.Entities;
    public sealed class VerificationService : IVerificationService
    {
        private readonly IVerificationTokenRepository _tokens;
        private readonly IUnitOfWork _uow;
        private readonly IClock _clock;

        public VerificationService(
            IVerificationTokenRepository tokens,
            IUnitOfWork uow,
            IClock clock)
        {
            _tokens = tokens;
            _uow = uow;
            _clock = clock;
        }

        public async Task<(Guid VerificationId, string RawTokenOrOtp)> CreateAsync(
            Guid customerId, string channelValue, string type, string? correlationId, int ttlMinutes, CancellationToken ct)
        {
            // Steps:
            // 1) Generate token/OTP (raw); hash for storage
            // 2) Create VerificationToken entity with expiry, attempts=0
            // 3) Persist and return (VerificationId, rawSecret) for sending
            throw new NotImplementedException();
        }

        public async Task<bool> VerifyAsync(Guid customerId, Guid verificationId, string presentedSecret, CancellationToken ct)
        {
            // Steps:
            // 1) Load token; check customerId match, expiry, attempts
            // 2) Hash presentedSecret and compare; update status/attempts
            // 3) Persist; return true/false
            throw new NotImplementedException();
        }
    }
}
