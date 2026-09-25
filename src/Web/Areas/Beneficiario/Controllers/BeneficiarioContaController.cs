using System.Security.Claims;
using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Web.Areas.Beneficiario.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Beneficiario.Controllers;

[Area("Beneficiario")]
public class BeneficiarioContaController(AcessoBeneficiarioApiClient acessos) : Controller
{
    [AllowAnonymous, HttpGet]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        var sessao = await HttpContext.AuthenticateAsync(BeneficiarioAuthentication.Scheme);
        if (sessao.Succeeded && sessao.Principal?.IsInRole("Beneficiario") == true)
            return LocalRedirect("/Beneficiario/MinhaArea");
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginCpfBeneficiarioContaVm());
    }

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginCpfBeneficiarioContaVm model, string? returnUrl, CancellationToken ct)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);
        try
        {
            var resultado = await acessos.LoginCpfAsync(new LoginCpfBeneficiarioVm { Cpf = model.Cpf, Senha = model.Senha }, ct);
            if (resultado.SemCadastro)
            {
                ViewData["SemCadastro"] = true;
                return View(model);
            }
            var login = resultado.Login;
            if (login is null || login.Usuario.Role != Role.Beneficiario || login.Usuario.BeneficiarioId is null)
            {
                ModelState.AddModelError("", "CPF ou senha inválidos.");
                return View(model);
            }
            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, login.Usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, login.Usuario.NomeCompleto ?? "Beneficiário"),
                new Claim(ClaimTypes.Email, login.Usuario.Email ?? ""),
                new Claim(ClaimTypes.Role, Role.Beneficiario.ToString())
            }, BeneficiarioAuthentication.Scheme);
            var properties = new AuthenticationProperties { IsPersistent = false };
            properties.StoreTokens([new AuthenticationToken { Name = "access_token", Value = login.Token }]);
            await HttpContext.SignInAsync(BeneficiarioAuthentication.Scheme, new ClaimsPrincipal(identity), properties);
            return LocalRedirect(Url.IsLocalUrl(returnUrl) && returnUrl!.StartsWith("/Beneficiario/MinhaArea", StringComparison.OrdinalIgnoreCase)
                ? returnUrl : "/Beneficiario/MinhaArea");
        }
        catch (HttpRequestException)
        { ModelState.AddModelError("", "Não foi possível entrar. Tente novamente em instantes."); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { ModelState.AddModelError("", "A API demorou para responder. Tente novamente."); }
        return View(model);
    }

    [AllowAnonymous, HttpGet]
    public IActionResult Cadastro() => View(new CadastroBeneficiarioContaVm());

    [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Cadastro(CadastroBeneficiarioContaVm model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            var status = await acessos.RegistrarAsync(new CadastroAcessoBeneficiarioVm
                { Cpf = model.Cpf, Senha = model.Senha }, ct);
            if (status == HttpStatusCode.BadRequest)
            {
                ModelState.AddModelError(nameof(model.Cpf), "Não existe carteirinha para o CPF informado. Solicite primeiro sua carteirinha.");
                return View(model);
            }
            if (status == HttpStatusCode.Conflict)
            {
                ModelState.AddModelError("", "Este CPF já possui acesso. Entre com sua senha ou fale com a equipe da PVH Saúde.");
                return View(model);
            }
            TempData["CadastroConcluido"] = "Conta criada. Entre com seu CPF e a senha que você definiu.";
            return RedirectToAction(nameof(Login));
        }
        catch (HttpRequestException)
        { ModelState.AddModelError("", "Não foi possível criar sua conta. Tente novamente."); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        { ModelState.AddModelError("", "A API demorou para responder. Tente novamente."); }
        return View(model);
    }

    [Authorize(AuthenticationSchemes = BeneficiarioAuthentication.Scheme, Roles = "Beneficiario"), HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(BeneficiarioAuthentication.Scheme);
        return RedirectToAction(nameof(Login));
    }
}
