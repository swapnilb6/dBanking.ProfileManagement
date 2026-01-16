using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class Address
    {
        public Guid AddressId { get; set; } = Guid.NewGuid();
        public Guid CustomerId { get; set; }             // FK to Profile.CustomerId

        public AddressType AddressType { get; set; } = AddressType.Residential;

        public string Line1 { get; set; } = string.Empty;
        public string? Line2 { get; set; }
        public string? Line3 { get; set; }
        public string City { get; set; } = string.Empty;
        public string StateProvince { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string CountryCode { get; set; } = "IN";  // ISO 3166-1 alpha-2

        public bool IsPrimary { get; set; } = true;
        public DateTimeOffset EffectiveFrom { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EffectiveTo { get; set; }

        // Audit
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
    }
}
