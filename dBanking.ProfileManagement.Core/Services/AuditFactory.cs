using dBanking.ProfileManagement.Core.Entities;
using dBanking.ProfileManagement.Core.Services.Internals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dBanking.ProfileManagement.Core.Services
{
    public static class AuditFactory
    {
        public static AuditRecord Create(
            Guid customerId,
            string entityName,
            string entityId,
            string action,
            object? oldObject,
            object? newObject,
            string? changedFieldsCsv,
            ActorContext actor,
            DateTimeOffset now)
        {
            return new AuditRecord
            {
                CustomerId = customerId,
                EntityName = entityName,
                EntityId = entityId,
                Action = action,

                // Map new names:
                Actor = actor.ActorId,                 // was ActorId
                Channel = actor.SourceChannel.ToString(),// was SourceChannel
                ChangedAt = now,                           // was Timestamp

                OldJson = JsonHelper.ToJson(oldObject),
                NewJson = JsonHelper.ToJson(newObject),
                ChangedFieldsCsv = changedFieldsCsv,

                CorrelationId = actor.CorrelationId
            };
        }

        // Optional: if you want to carry Reason/IP/UA without new DB columns,
        // you can merge them into NewJson metadata.
        public static AuditRecord CreateWithMeta(
            Guid customerId,
            string entityName,
            string entityId,
            string action,
            object? oldObject,
            object? newObject,
            string? changedFieldsCsv,
            ActorContext actor,
            DateTimeOffset now,
            bool embedMeta = true)
        {
            var finalNew = newObject;

            if (embedMeta && (actor.Reason != null || actor.IpAddress != null || actor.UserAgent != null))
            {
                finalNew = new
                {
                    data = newObject,
                    _meta = new
                    {
                        reason = actor.Reason,
                        ip = actor.IpAddress,
                        ua = actor.UserAgent
                    }
                };
            }

            return Create(customerId, entityName, entityId, action, oldObject, finalNew, changedFieldsCsv, actor, now);
        }
    }
}
