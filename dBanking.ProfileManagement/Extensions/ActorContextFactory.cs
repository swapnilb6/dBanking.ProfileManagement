
using dBanking.ProfileManagement.Core.Entities;
using Microsoft.AspNetCore.Http;
using static dBanking.ProfileManagement.API.Extensions.HttpContextExtensions;

namespace dBanking.ProfileManagement.API.Extensions
{
    public static class ActorContextFactory
    {
        public static ActorContext From(HttpContext ctx, string? reason = null)
        {
            var actorId = ctx.GetActorId() ?? "anonymous";
            var role = ctx.User.IsInRole("CSA") ? ActorRole.CSA :
                       ctx.User.IsInRole("Supervisor") ? ActorRole.Supervisor :
                       ctx.User.Identity?.IsAuthenticated == true ? ActorRole.Customer : ActorRole.System;

            // You can map channel from client id or header; defaulting to Web.
            var sourceChannel = SourceChannel.Web;

            return new ActorContext(
                ActorId: actorId,
                ActorRole: role,
                SourceChannel: sourceChannel,
                IpAddress: ctx.GetSourceIp(),
                UserAgent: ctx.GetUserAgent(),
                CorrelationId: ctx.GetOrCreateCorrelationId(),
                Reason: reason
            );
        }
    }
}
