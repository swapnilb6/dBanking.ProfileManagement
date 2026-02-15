using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Entities
{
    public sealed class AuditRecord
    {


        public long Id { get; set; }
        public Guid CustomerId { get; set; }
        public string EntityName { get; set; } = default!;
        public string EntityId { get; set; } = default!;
        public string Action { get; set; } = default!;
        public string? Actor { get; set; }
        public string? Channel { get; set; }
        public DateTimeOffset ChangedAt { get; set; }
        public string? OldJson { get; set; }
        public string? NewJson { get; set; }
        public string? ChangedFieldsCsv { get; set; }
        public Guid? CorrelationId { get; set; }

    }
}
