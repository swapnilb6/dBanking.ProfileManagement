using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class VerificationToken
    {
        public long Id { get; set; }
        public Guid CustomerId { get; set; }

        public string Type { get; set; } = default!;      // 'EmailLink'|'SmsOtp'
        public string ChannelValue { get; set; } = default!;
        public string TokenHash { get; set; } = default!;
        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }

        public int AttemptCount { get; set; }
        public int MaxAttempts { get; set; }
        public string Status { get; set; } = "Pending";   // 'Pending'|...

        public DateTimeOffset? VerifiedAt { get; set; }
        public Guid? CorrelationId { get; set; }

    }
}
