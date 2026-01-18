
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using dBanking.ProfileManagement.API.Extensions;

namespace dBanking.ProfileManagement.API.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class IdempotencyRequiredAttribute : Attribute, IAsyncActionFilter
    {
        private readonly TimeSpan _ttl;
        public const string HeaderName = "Idempotency-Key";

        public IdempotencyRequiredAttribute(int ttlMinutes = 30)
        {
            _ttl = TimeSpan.FromMinutes(ttlMinutes);
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<IdempotencyRequiredAttribute>>();
            var store = context.HttpContext.RequestServices.GetRequiredService<IIdempotencyStore>();
            var http = context.HttpContext;

            // Require header
            if (!http.Request.Headers.TryGetValue(HeaderName, out var key) || string.IsNullOrWhiteSpace(key))
            {
                context.Result = new BadRequestObjectResult(new { code = "IDEMPOTENCY_KEY_REQUIRED", message = "Idempotency-Key header is required." });
                return;
            }

            var actor = http.GetActorId() ?? "anonymous";
            var existing = await store.GetAsync(actor, key!, http.RequestAborted);
            if (existing is not null)
            {
                logger.LogInformation("Idempotent replay detected for key {Key}. Returning stored response.", key.ToString());
                http.Response.StatusCode = existing.StatusCode;
                http.Response.ContentType = existing.ContentType;
                await http.Response.WriteAsync(existing.Body);
                return;
            }

            // Capture response
            var originalBody = http.Response.Body;
            await using var memStream = new MemoryStream();
            http.Response.Body = memStream;

            var executed = await next();

            memStream.Seek(0, SeekOrigin.Begin);
            var bodyText = await new StreamReader(memStream, Encoding.UTF8).ReadToEndAsync();
            memStream.Seek(0, SeekOrigin.Begin);

            await store.SetAsync(actor, key!, new IdempotencyEntry(
                StatusCode: http.Response.StatusCode,
                ContentType: http.Response.ContentType ?? "application/json",
                Body: bodyText,
                CreatedAt: DateTimeOffset.UtcNow), _ttl, http.RequestAborted);

            await memStream.CopyToAsync(originalBody);
            http.Response.Body = originalBody;
        }
    }
}