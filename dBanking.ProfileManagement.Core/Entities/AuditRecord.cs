using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class AuditRecord
    {
        public Guid AuditId { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }

        public string Entity { get; set; } = string.Empty;    // Contacts | Address | Preferences
        public string EntityId { get; set; } = string.Empty;  // AddressId or logical key
        public string Operation { get; set; } = string.Empty; // Create | Update | Verify | Delete

        // Store old/new snapshots as JSON (immutable append-only)
        public string OldValueJson { get; set; } = "{}";
        public string NewValueJson { get; set; } = "{}";
        public string ChangedFieldsCsv { get; set; } = string.Empty;

        public string ActorId { get; set; } = string.Empty;   // subject identifier (OID / sub)
        public ActorRole ActorRole { get; set; } = ActorRole.Customer;
        public SourceChannel SourceChannel { get; set; } = SourceChannel.Web;

        public string? ReasonCode { get; set; }               // optional reason provided by CSA
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? CorrelationId { get; set; }

        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
    }
}
