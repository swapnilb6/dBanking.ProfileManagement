
using dBanking.ProfileManagement.Core.ServiceContracts;
using dBanking.ProfileManagement.Core.Services;
using dBanking.ProfileManagement.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
namespace dBanking.ProfileManagement.Core
{
    public static class dependancyInjection
    {
        // Extension method to add core services to the IServiceCollection
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {

            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IAddressService, AddressService>();
            services.AddScoped<IPreferenceService, PreferenceService>();
            services.AddScoped<IVerificationService, VerificationService>();
            services.AddScoped<IProfileEventPublisher, ProfileEventPublisher>();
            services.AddSingleton<IClock, SystemClock>();
            return services;
        }
    }
}

