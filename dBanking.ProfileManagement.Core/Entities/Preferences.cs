using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class Preferences
    {
        public bool SmsEnabled { get; set; } = true;
        public bool EmailEnabled { get; set; } = true;
        public bool PushEnabled { get; set; } = true;

        // Regulatory/consent flags — extend as bit flags or a separate table if needed
        public bool RegulatoryConsentGiven { get; set; } = false;

        // Optional personalization
        public string? Language { get; set; }            // e.g., "en-IN"
        public string? TimeZone { get; set; }            // e.g., "Asia/Kolkata"

        public DateTimeOffset UpdatedAt { get; set; }
    }
}
