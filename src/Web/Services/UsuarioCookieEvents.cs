using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
namespace Web.Services;

public class UsuarioCookieEvents(IHttpClientFactory factory) : CookieAuthenticationEvents
{
    public override Task RedirectToLogin(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (context.Scheme.Name == BeneficiarioAuthentication.Scheme)
        {
            var login = "/Beneficiario/BeneficiarioConta/Login";
            if (HttpMethods.IsGet(context.Request.Method) && context.Request.Path.StartsWithSegments("/Beneficiario/MinhaArea"))
                login += "?returnUrl=" + Uri.EscapeDataString(context.Request.Path + context.Request.QueryString);
            context.Response.Redirect(login);
            return Task.CompletedTask;
        }
        return base.RedirectToLogin(context);
    }

    public override Task RedirectToAccessDenied(RedirectContext<CookieAuthenticationOptions> context)
    {
        if (context.Scheme.Name == BeneficiarioAuthentication.Scheme)
        {
            context.Response.Redirect("/Beneficiario/BeneficiarioConta/Login");
            return Task.CompletedTask;
        }
        return base.RedirectToAccessDenied(context);
    }

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
            if (response.IsSuccessStatusCode)
            {
                var menus = await response.Content.ReadFromJsonAsync<string[]>(timeout.Token);
                if (menus is not null && context.Principal?.Identity is System.Security.Claims.ClaimsIdentity identity)
                {
                    PVHSAUDE.Application.ViewModels.AcessoMenu.Atualizar(identity, menus);
                    return;
                }
            }
        }
        catch (HttpRequestException) { }
        catch (System.Text.Json.JsonException) { }
        catch (OperationCanceledException) when (!context.HttpContext.RequestAborted.IsCancellationRequested) { }
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(context.Scheme.Name);
    }
}
