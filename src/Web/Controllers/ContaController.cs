using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.ViewModels;
using Web.Services;
using PVHSAUDE.Domain.Enuns;
namespace PVHSAUDE.Web.Controllers;

public class ContaController(UsuarioApiClient usuarios) : Controller
{
    [AllowAnonymous, HttpGet]
    public IActionResult Login(string? returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(new LoginVm()); }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm model, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);
        try
        {
            var login = await usuarios.LoginAsync(model, ct);
            if (login is null || login.Usuario.Role == Role.Beneficiario) { ModelState.AddModelError("", "E-mail ou senha inválidos."); return View(model); }
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, login.Usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, login.Usuario.NomeCompleto!),
                new Claim(ClaimTypes.Email, login.Usuario.Email!),
                new Claim(ClaimTypes.Role, login.Usuario.Role.ToString())
            };
            var properties = new AuthenticationProperties { IsPersistent = false };
            properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = login.Token }]);
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AcessoMenu.Atualizar(identity, login.Usuario.Menus);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity), properties);
            return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Administracao");
        }
        catch (HttpRequestException) { ModelState.AddModelError("", "Não foi possível entrar. Verifique a conexão com a API e tente novamente."); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { ModelState.AddModelError("", "A API demorou para responder. Tente novamente."); }
        return View(model);
    }
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        var eraBeneficiario = User.IsInRole("Beneficiario");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return eraBeneficiario
            ? RedirectToAction("Login", "BeneficiarioConta", new { area = "Beneficiario" })
            : RedirectToAction(nameof(Login));
    }
    [AllowAnonymous]
    public IActionResult AcessoNegado() => View();
}
