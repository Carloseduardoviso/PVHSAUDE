using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;
[Area("Administracao")]
public class DescontoController(DescontoApiClient descontos) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try { return View(await descontos.ListarAsync(ct)); }
        catch (HttpRequestException) { TempData["Error"] = "Não foi possível carregar os descontos."; return View(Array.Empty<CatalogoItemVm>()); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(string nome, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(nome))
            try { await descontos.CriarAsync(nome, ct); TempData["Success"] = "Desconto cadastrado com sucesso."; }
            catch (HttpRequestException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(Guid id, string nome, CancellationToken ct)
    {
        try { await descontos.AtualizarAsync(id, nome, ct); TempData["Success"] = "Desconto atualizado com sucesso."; }
        catch (HttpRequestException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Excluir(Guid id, CancellationToken ct)
    {
        try { await descontos.ExcluirAsync(id, ct); TempData["Success"] = "Desconto excluído com sucesso."; }
        catch (HttpRequestException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
