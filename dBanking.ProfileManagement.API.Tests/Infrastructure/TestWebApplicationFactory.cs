using dBanking.ProfileManagement.API;
using dBanking.ProfileManagement.API.Filters;
using dBanking.ProfileManagement.Core.DTOs;
using dBanking.ProfileManagement.Core.ServiceContracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;

namespace dBanking.ProfileManagement.API.Tests.Infrastructure
{
    public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IContactService> ContactServiceMock { get; } = new();
        public Mock<IAddressService> AddressServiceMock { get; } = new();
        public Mock<IPreferenceService> PreferenceServiceMock { get; } = new();
        public Mock<IAuditService> AuditServiceMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace Authentication with Test scheme
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });

                // Replace the DI registrations with mocks
                services.RemoveAll<IContactService>();
                services.RemoveAll<IAddressService>();
                services.RemoveAll<IPreferenceService>();
                services.RemoveAll<IAuditService>();
                services.RemoveAll<IIdempotencyStore>();

                services.AddSingleton(ContactServiceMock.Object);
                services.AddSingleton(AddressServiceMock.Object);
                services.AddSingleton(PreferenceServiceMock.Object);
                services.AddSingleton(AuditServiceMock.Object);

                // Use in-memory idempotency store (same as API used)
                services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();
            });
        }
    }
}