using dBanking.ProfileManagement.Core.RepositoryContracts;
using dBanking.ProfileManagement.Infrastructure.DbContext;
using dBanking.ProfileManagement.Infrastructure.Repository;
using dBanking.ProfileManagement.Infrastructure.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace dBanking.ProfileManagement.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddProfileInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration,
            string connectionStringName = "ProfileDb")
        {
            // Repositories
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IVerificationTokenRepository, VerificationTokenRepository>();
            services.AddScoped<IAuditRepository, AuditRepository>();

            // UoW
            services.AddScoped<IUnitOfWork, EfUnitOfWork>();

            return services;
        }
    }
}