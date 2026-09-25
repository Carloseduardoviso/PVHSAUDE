using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PVHSAUDE.Application.AppService;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Infra.Helper.Settings;
using System.Text;

namespace PVHSAUDE.Api.Configs
{
    public static class AuthServiceConfig
    {
        private static async Task<UsuarioVm?> ObterUsuario(IUsuarioService service, Guid id, CancellationToken ct)
        {
            try { return await service.ObterAsync(id, ct); }
            catch (ServiceException ex)
                when (ex.Error == ServiceError.NotFound)
            { return null; }
        }

        public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services)
        {
            var jwtSettings = services.BuildServiceProvider().GetRequiredService<IOptions<JwtSetting>>().Value;

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var id = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                        var service = context.HttpContext.RequestServices.GetRequiredService<IUsuarioService>();
                        var usuario = Guid.TryParse(id, out var guid)
                            ? await ObterUsuario(service, guid, context.HttpContext.RequestAborted)
                            : null;
                        if (usuario is null || !usuario.Ativo ||
                            usuario.Role == PVHSAUDE.Domain.Enuns.Role.Beneficiario && usuario.BeneficiarioId is null ||
                            context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value != usuario.Role.ToString())
                            context.Fail("Usuário indisponível ou permissão alterada.");
                        else if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
                            AcessoMenu.Atualizar(identity, usuario.Menus);
                    }
                };
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,

                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            return services;
        }
    }
}
