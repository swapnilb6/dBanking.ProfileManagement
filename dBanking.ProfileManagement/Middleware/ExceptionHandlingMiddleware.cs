using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using dBanking.ProfileManagement.API.Extensions;
using FluentValidation;

namespace dBanking.ProfileManagement.API.Middleware
{
    public sealed class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException vex)
            {
                // FV exceptions (if thrown from services)
                await WriteProblem(context, HttpStatusCode.BadRequest, "ValidationError", vex.Message, vex.Errors);
            }
            catch (KeyNotFoundException kex)
            {
                await WriteProblem(context, HttpStatusCode.NotFound, "NotFound", kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                await WriteProblem(context, HttpStatusCode.Unauthorized, "Unauthorized", uex.Message);
            }
            catch (InvalidOperationException iex)
            {
                await WriteProblem(context, HttpStatusCode.Conflict, "Conflict", iex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteProblem(context, HttpStatusCode.InternalServerError, "ServerError", "An unexpected error occurred.");
            }
        }

        private async Task WriteProblem(HttpContext ctx, HttpStatusCode code, string title, string detail, object? errors = null)
        {
            var correlationId = ctx.Items[HttpContextExtensions.CorrelationIdHeader] as string
                                ?? ctx.GetOrCreateCorrelationId();

            var problem = new
            {
                type = $"https://httpstatuses.com/{(int)code}",
                title,
                status = (int)code,
                detail,
                correlationId,
                traceId = ctx.TraceIdentifier,
                errors
            };

            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.StatusCode = (int)code;

            await ctx.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
