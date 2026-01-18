using Microsoft.AspNetCore.Http;
namespace dBanking.ProfileManagement.API.Extensions
{
    public static class HttpContextExtensions
    {
        public const string CorrelationIdHeader = "X-Correlation-Id";

        public static string GetOrCreateCorrelationId(this HttpContext ctx)
        {
            if (!ctx.Request.Headers.TryGetValue(CorrelationIdHeader, out var cid) ||
                string.IsNullOrWhiteSpace(cid))
            {
                cid = Guid.NewGuid().ToString("n");
            }
            return cid!;
        }

        public static void SetCorrelationId(this HttpContext ctx, string correlationId)
        {
            ctx.Response.Headers[CorrelationIdHeader] = correlationId;
        }

        public static string? GetActorId(this HttpContext ctx)
            => ctx.User?.Identity?.IsAuthenticated == true
                ? ctx.User.FindFirst("oid")?.Value
                    ?? ctx.User.FindFirst("sub")?.Value
                : null;

        public static string GetSourceIp(this HttpContext ctx)
            => ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        public static string? GetUserAgent(this HttpContext ctx)
            => ctx.Request.Headers.UserAgent.ToString();
    }
}
