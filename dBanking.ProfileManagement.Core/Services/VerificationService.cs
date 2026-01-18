using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using dBanking.ProfileManagement.Core.Entities;
using dBanking.ProfileManagement.Core.RepositoryContracts;
using dBanking.ProfileManagement.Core.ServiceContracts;

namespace dBanking.ProfileManagement.Core.Services
{
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
            var vType = type.Equals("EmailLink", StringComparison.OrdinalIgnoreCase)
                        ? VerificationType.EmailLink
                        : VerificationType.SmsOtp;

            // Generate secret (token or OTP)
            string secret;
            if (vType == VerificationType.SmsOtp)
            {
                // 6-digit numeric OTP
                var rng = RandomNumberGenerator.Create();
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                var value = BitConverter.ToUInt32(bytes, 0) % 1000000;
                secret = value.ToString("D6");
            }
            else
            {
                // URL-friendly token
                var bytes = RandomNumberGenerator.GetBytes(32);
                secret = Convert.ToBase64String(bytes)
                                 .TrimEnd('=')
                                 .Replace('+', '-')
                                 .Replace('/', '_');
            }

            var token = new VerificationToken
            {
                VerificationId = Guid.NewGuid(),
                CustomerId = customerId,
                Type = vType,
                ChannelValue = channelValue,
                TokenHash = Sha256(secret),
                Status = VerificationStatus.Pending,
                ExpiresAt = _clock.UtcNow.AddMinutes(ttlMinutes),
                AttemptCount = 0,
                MaxAttempts = vType == VerificationType.SmsOtp ? 5 : 10,
                CorrelationId = correlationId,
                CreatedAt = _clock.UtcNow
            };

            await _tokens.AddAsync(token, ct);
            await _uow.SaveChangesAsync(ct);

            return (token.VerificationId, secret);
        }

        public async Task<bool> VerifyAsync(Guid customerId, Guid verificationId, string presentedSecret, CancellationToken ct)
        {
            // NOTE: Since controllers don't have verificationId,
            // we allow verifying the latest pending token for this customer & type (in service usage context).
            // For safety, we first try by id if provided.
            VerificationToken? token = null;

            if (verificationId != Guid.Empty)
            {
                token = await _tokens.GetByIdAsync(verificationId, ct);
            }
            if (token is null)
            {
                // Fallback: get pending token for the customer (most recent for either type)
                // Repositories could be extended to fetch latest by CreatedAt; for now, GetPendingAsync is per type.
                // We'll attempt verify against both types (EmailLink first)
                token = await _tokens.GetPendingAsync(customerId, VerificationType.EmailLink, ct)
                        ?? await _tokens.GetPendingAsync(customerId, VerificationType.SmsOtp, ct);
            }

            if (token is null) return false;
            if (token.CustomerId != customerId) return false;
            if (token.Status != VerificationStatus.Pending) return false;
            if (_clock.UtcNow > token.ExpiresAt)
            {
                token.Status = VerificationStatus.Expired;
                await _tokens.UpdateAsync(token, ct);
                await _uow.SaveChangesAsync(ct);
                return false;
            }

            token.AttemptCount++;
            if (token.AttemptCount > token.MaxAttempts)
            {
                token.Status = VerificationStatus.Failed;
                token.FailureReason = "AttemptsExceeded";
                await _tokens.UpdateAsync(token, ct);
                await _uow.SaveChangesAsync(ct);
                return false;
            }

            var ok = token.TokenHash == Sha256(presentedSecret);
            if (!ok)
            {
                await _tokens.UpdateAsync(token, ct);
                await _uow.SaveChangesAsync(ct);
                return false;
            }

            token.Status = VerificationStatus.Verified;
            token.VerifiedAt = _clock.UtcNow;
            await _tokens.UpdateAsync(token, ct);
            await _uow.SaveChangesAsync(ct);

            return true;
        }

        private static string Sha256(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}