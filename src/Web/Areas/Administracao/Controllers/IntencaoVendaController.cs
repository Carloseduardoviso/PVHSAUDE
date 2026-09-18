using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Domain.Enuns;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao"), Authorize]
public class IntencaoVendaController(IntencaoVendaApiClient api) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try { return View(await api.ListarAsync(ct)); }
        catch (HttpRequestException) { ViewData["Erro"] = "Não foi possível carregar as intenções de venda."; return View(new List<IntencaoVendaVm>()); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AtualizarStatus(Guid id, StatusIntencaoVenda status, CancellationToken ct)
    {
        try { await api.AtualizarStatusAsync(id, status, ct); TempData["Sucesso"] = "Situação atualizada."; }
        catch (HttpRequestException) { TempData["Erro"] = "Não foi possível atualizar a situação."; }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Detalhes(Guid id, CancellationToken ct)
    {
        try { var item = (await api.ListarAsync(ct)).FirstOrDefault(x => x.Id == id); return item is null ? NotFound() : View(item); }
        catch (HttpRequestException) { return NotFound(); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspenderNotificacao(Guid id, CancellationToken ct)
    {
        try { await api.SuspenderNotificacaoAsync(id, ct); TempData["Sucesso"] = "Notificação da intenção suspensa."; }
        catch (HttpRequestException) { TempData["Erro"] = "Não foi possível suspender a notificação."; }
        return RedirectToAction(nameof(Index));
    }
}
