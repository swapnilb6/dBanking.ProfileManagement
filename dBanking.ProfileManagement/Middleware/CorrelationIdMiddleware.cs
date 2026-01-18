using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using static dBanking.ProfileManagement.API.Extensions.HttpContextExtensions;

namespace dBanking.ProfileManagement.API.Middleware
{
    public sealed class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.GetOrCreateCorrelationId();
            context.Items[CorrelationIdHeader] = correlationId;

            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["Path"] = context.Request.Path.Value,
                ["Method"] = context.Request.Method
            });

            context.SetCorrelationId(correlationId);
            await _next(context);
        }
    }
}
