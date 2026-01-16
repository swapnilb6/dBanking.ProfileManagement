using System;
using System.Collections.Generic;


namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class Profile
    {
        // Aggregate root identity — mirrors CustomerId from Party/Customer domain
        public Guid CustomerId { get; set; }

        public Contact Contact { get; set; } = new();
        public Preferences Preferences { get; set; } = new();
        public List<Address> Addresses { get; set; } = new();

        // Concurrency & audit
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public byte[] RowVersion { get; set; } // EF Core optimistic concurrency

        // Utility
        public bool IsNew() => CreatedAt == default;
    }
}
