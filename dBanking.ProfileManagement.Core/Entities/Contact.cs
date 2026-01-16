using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class Contact
    {
        // Current verified/active values
        public string? Email { get; set; }               // store canonical case
        public ContactStatus EmailStatus { get; set; }   // Verified/Pending
        public string? PhoneE164 { get; set; }           // +<country><number>
        public ContactStatus PhoneStatus { get; set; }

        // Pending change requests
        public string? PendingEmail { get; set; }
        public DateTimeOffset? PendingEmailRequestedAt { get; set; }

        public string? PendingPhoneE164 { get; set; }
        public DateTimeOffset? PendingPhoneRequestedAt { get; set; }

        // Operational metadata
        public DateTimeOffset LastEmailVerifiedAt { get; set; }
        public DateTimeOffset LastPhoneVerifiedAt { get; set; }
    }
}
