using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class Address
    {
        public long Id { get; set; }                // surrogate
        public Guid AddressId { get; set; }         // public id
        public Guid CustomerId { get; set; }        // FK -> Profile.CustomerId

        public AddressType AddressType { get; set; } = default!;
        public string Type { get; set; } = "Residential";
        public string Line1 { get; set; } = default!;
        public string? Line2 { get; set; }
        public string? Line3 { get; set; }
        public string City { get; set; } = default!;
        public string? StateProvince { get; set; }
        public string? PostalCode { get; set; }
        public string CountryCode { get; set; } = default!;

        public bool IsPrimary { get; set; }

        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

    }
}
