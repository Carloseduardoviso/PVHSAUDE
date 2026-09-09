using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;
namespace Web.Services;

public class ApiAuthenticationHandler(IHttpContextAccessor accessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.Headers.Authorization is null && accessor.HttpContext is { } context && context.User.Identity?.IsAuthenticated == true)
        {
            var token = await context.GetTokenAsync("access_token");
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        return await base.SendAsync(request, ct);
    }
}
