using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
namespace Web.Services;

public class UsuarioCookieEvents(IHttpClientFactory factory) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        using var client = factory.CreateClient("default");
        using var request = new HttpRequestMessage(HttpMethod.Get, "Auth/sessao");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.Properties.GetTokenValue("access_token"));
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.HttpContext.RequestAborted);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        try
        {
            using var response = await client.SendAsync(request, timeout.Token);
            if (response.IsSuccessStatusCode) return;
        }
        catch (HttpRequestException) { }
        catch (OperationCanceledException) when (!context.HttpContext.RequestAborted.IsCancellationRequested) { }
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
