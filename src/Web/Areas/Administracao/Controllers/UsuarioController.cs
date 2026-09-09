using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.ViewModels;
using Web.Services;
namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao"), Authorize(Roles = "Administrador")]
public class UsuarioController(UsuarioApiClient usuarios) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try { return View(await usuarios.ListarAsync(ct)); }
        catch (HttpRequestException) { ViewData["Erro"] = "Não foi possível carregar os usuários. Verifique a API e tente novamente."; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { ViewData["Erro"] = "A API demorou para responder. Tente novamente."; }
        return View(new List<UsuarioVm>());
    }
    [HttpGet]
    public IActionResult Create() => View(new UsuarioCadastroVm());
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        try
        {
            var u = await usuarios.ObterAsync(id, ct);
            if (u is null) return NotFound();
            return View(new UsuarioEdicaoVm { UsuarioId = u.UsuarioId, NomeCompleto = u.NomeCompleto!, Email = u.Email!, Role = u.Role, Menus = u.Menus });
        }
        catch (HttpRequestException) { TempData["Error"] = "Não foi possível carregar o usuário. Tente novamente."; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { TempData["Error"] = "A API demorou para responder."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, UsuarioEdicaoVm model, CancellationToken ct)
    {
        if (id != model.UsuarioId) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        try
        {
            var erro = await usuarios.AlterarAsync(id, model, "", ct);
            if (erro is null) { TempData["Success"] = "Usuário atualizado."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError("", erro);
        }
        catch (HttpRequestException) { ModelState.AddModelError("", "Não foi possível atualizar o usuário. Tente novamente."); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { ModelState.AddModelError("", "A API demorou para responder."); }
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Inativar(Guid id, CancellationToken ct) => AlterarStatus(id, "inativar", "Usuário inativado.", ct);
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Ativar(Guid id, CancellationToken ct) => AlterarStatus(id, "ativar", "Usuário reativado.", ct);
    [HttpPost, ValidateAntiForgeryToken]
    public Task<IActionResult> Excluir(Guid id, CancellationToken ct) => AlterarStatus(id, "excluir", "Usuário excluído.", ct);

    private async Task<IActionResult> AlterarStatus(Guid id, string acao, string sucesso, CancellationToken ct)
    {
        try
        {
            var erro = await usuarios.AlterarAsync(id, null, acao, ct);
            TempData[erro is null ? "Success" : "Error"] = erro ?? sucesso;
        }
        catch (HttpRequestException) { TempData["Error"] = "Não foi possível concluir a operação. Tente novamente."; }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { TempData["Error"] = "A API demorou para responder."; }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCadastroVm model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            if (await usuarios.CriarAsync(model, ct))
            {
                TempData["Success"] = "Usuário cadastrado com sucesso.";
                return RedirectToAction(nameof(Index));
            }
            ModelState.AddModelError(nameof(model.Email), "Já existe um usuário com este e-mail.");
        }
        catch (HttpRequestException) { ModelState.AddModelError("", "Não foi possível cadastrar o usuário. Verifique a API e tente novamente."); }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) { ModelState.AddModelError("", "A API demorou para responder. Tente novamente."); }
        return View(model);
    }
}
