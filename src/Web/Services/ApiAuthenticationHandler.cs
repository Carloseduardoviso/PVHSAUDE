using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
namespace Web.Services;

public class ApiAuthenticationHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.Headers.Authorization is null && accessor.HttpContext is { } context)
        {
            var scheme = context.Request.Path.StartsWithSegments("/Beneficiario")
                ? BeneficiarioAuthentication.Scheme
                : Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme;
            var token = await context.GetTokenAsync(scheme, "access_token");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, ct);
    }
}
