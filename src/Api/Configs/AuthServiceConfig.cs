using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PVHSAUDE.Infra.Helper.Settings;
using System.Text;

namespace PVHSAUDE.Api.Configs
{
    public static class AuthServiceConfig
    {
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
                        var db = context.HttpContext.RequestServices.GetRequiredService<global::Infra.Data.Base.Context>();
                        var usuario = Guid.TryParse(id, out var guid)
                            ? await db.Set<PVHSAUDE.Domain.Entities.Usuario>().FindAsync(new object[] { guid }, context.HttpContext.RequestAborted)
                            : null;
                        if (usuario is null || !usuario.Ativo ||
                            context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value != usuario.Role.ToString())
                            context.Fail("Usuário indisponível ou permissão alterada.");
                        else if (context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
                            PVHSAUDE.Application.ViewModels.AcessoMenu.Atualizar(identity,
                                PVHSAUDE.Domain.Enuns.MenusAdministrativos.Ler(usuario.MenusPermitidos));
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
