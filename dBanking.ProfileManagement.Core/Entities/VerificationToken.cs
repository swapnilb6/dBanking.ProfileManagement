using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class VerificationToken
    {
        public Guid VerificationId { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }

        public VerificationType Type { get; set; }           // EmailLink or SmsOtp
        public string ChannelValue { get; set; } = string.Empty; // email or phoneE164 (new target)
        public string TokenHash { get; set; } = string.Empty; // store hash only
        public string? OtpSalt { get; set; }                  // optional if hashing OTP differently

        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public DateTimeOffset ExpiresAt { get; set; }
        public int AttemptCount { get; set; }
        public int MaxAttempts { get; set; } = 5;

        // Correlation & operations
        public string? CorrelationId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? VerifiedAt { get; set; }
        public string? FailureReason { get; set; }            // e.g., Expired, AttemptsExceeded
    }
}
