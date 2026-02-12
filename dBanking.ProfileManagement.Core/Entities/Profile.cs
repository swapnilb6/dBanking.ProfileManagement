using System;
using System.Collections.Generic;


namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class Profile
    {
        // Aggregate root identity — mirrors CustomerId from Party/Customer domain
        public long Id { get; set; }                   // surrogate
        public Guid CustomerId { get; set; }           // business key

        public string FirstName { get; set; } = default!;
        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string EmailStatus { get; set; } = "Unverified";
        public string? PendingEmail { get; set; }

        public string? PhoneE164 { get; set; }
        public string PhoneStatus { get; set; } = "Unverified";
        public string? PendingPhoneE164 { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;

        public bool? SmsEnabled { get; set; }
        public bool? EmailEnabled { get; set; }
        public bool? PushEnabled { get; set; }
        public bool? RegulatoryConsent { get; set; }
        public string? Language { get; set; }
        public string? TimeZone { get; set; }
        public DateTimeOffset? PreferencesUpdatedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

    }
}
