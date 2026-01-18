using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace dBanking.ProfileManagement.API.Tests.Infrastructure
{
    public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string Scheme = "Test";

        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock) : base(options, logger, encoder, clock) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Read requested scopes from header for flexibility (comma separated)
            var scopesHeader = Context.Request.Headers["X-Test-Scopes"].ToString();
            var scopes = string.IsNullOrWhiteSpace(scopesHeader) ? "profile.read profile.write" : scopesHeader.Replace(",", " ");

            var claims = new List<Claim>
            {
                new("sub", "test-user"),
                new("oid", "00000000-0000-0000-0000-000000000001"),
                new("scp", scopes)
            };
            var identity = new ClaimsIdentity(claims, Scheme);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
