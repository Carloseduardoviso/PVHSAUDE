using Infra.Auth;
using Microsoft.Extensions.DependencyInjection;
using PVHSAUDE.Domain.Interfaces.Repository;
using PVHSAUDE.Infra.Auth;
using PVHSAUDE.Infra.Auth.Interface;
using PVHSAUDE.Infra.Data;

namespace PVHSAUDE.Infra.Ioc
{
    public static class Modulo
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Application services

            // Domain services


            // Repositories
            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

            //Account
            services.AddScoped<IAccount, Account>();
            services.AddScoped<ProfileManager>();
            services.AddScoped<Access>();

            // Base
            services.AddScoped(typeof(IRepositoryBase<>), typeof(RepositoryBase<>));

            return services;
        }
    }
}
