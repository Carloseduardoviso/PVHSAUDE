using Microsoft.Extensions.Options;
using PVHSAUDE.Infra.Helper.Settings;
using System.Text;

namespace PVHSAUDE.Api.Middleware
{
    /// <summary>
    /// Middleware responsável proteger a rotas de metrics.
    /// </summary>
    public class MetricsAuthMiddleware(RequestDelegate next, IOptions<MetricsSettings> options)
    {
        private readonly RequestDelegate _next = next;
        private readonly MetricsSettings _settings = options.Value;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/metrics"))
            {
                var authHeader = context.Request.Headers.Authorization.FirstOrDefault();

                if (authHeader == null || !authHeader.StartsWith("Basic "))
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                var encoded = authHeader["Basic ".Length..].Trim();
                var credentials = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                var parts = credentials.Split(':');
                if (parts.Length != 2)
                {
                    context.Response.StatusCode = 404;
                    return;
                }

                var username = parts[0];
                var password = parts[1];

                if (username != _settings.Username || password != _settings.Password)
                {
                    context.Response.StatusCode = 404;
                    return;
                }
            }
            await _next(context);
        }
    }

    public static class UsaMetricsAuthMiddleware
    {
        /// <summary>
        /// Registra o ExceptionMiddleware no pipeline da aplicação.
        /// </summary>
        public static IApplicationBuilder UseMetricsAuthMiddleware(this IApplicationBuilder builder) => builder.UseMiddleware<MetricsAuthMiddleware>();
    }
}
