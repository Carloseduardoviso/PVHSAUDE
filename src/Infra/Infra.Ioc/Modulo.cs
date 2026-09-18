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
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<PVHSAUDE.Application.Interface.IPlanoService, PVHSAUDE.Application.AppService.PlanoService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IDescontoService, PVHSAUDE.Application.AppService.DescontoService>();
            services.AddScoped<PVHSAUDE.Application.Interface.ICatalogoService, PVHSAUDE.Application.AppService.CatalogoService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IBeneficiarioService, PVHSAUDE.Application.AppService.BeneficiarioService>();
            services.AddScoped<PVHSAUDE.Application.Interface.ICredenciadoService, PVHSAUDE.Application.AppService.CredenciadoService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IContatoService, PVHSAUDE.Application.AppService.ContatoService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IPortalService, PVHSAUDE.Application.AppService.PortalService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IUsuarioService, PVHSAUDE.Application.AppService.UsuarioService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IAuthService, PVHSAUDE.Application.AppService.AuthService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IBannerService, PVHSAUDE.Application.AppService.BannerService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IEmpresaBeneficiadaService, PVHSAUDE.Application.AppService.EmpresaBeneficiadaService>();
            services.AddScoped<PVHSAUDE.Application.Interface.IIntencaoVendaService, PVHSAUDE.Application.AppService.IntencaoVendaService>();
            return services;
        }
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // Application services

            // Domain services


            services.AddScoped(typeof(IEntityRepository<>), typeof(EntityRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IBannerRepository, BannerRepository>();
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
