using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class PlanoController(PlanoApiClient planos) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try { return View(await planos.ListarAsync(ct)); }
        catch (HttpRequestException) { ViewData["Error"] = "Não foi possível carregar os planos. Verifique se a API está disponível."; return View(Array.Empty<PlanoVm>()); }
    }

    [HttpGet]
    public IActionResult Create() => View(new PlanoVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlanoVm model, CancellationToken ct)
    {
        model.Id = Guid.Empty;
        if (!ModelState.IsValid) return View(model);
        try { await planos.SalvarAsync(model, ct); }
        catch (HttpRequestException ex) { ModelState.AddModelError("", ex.Message); return View(model); }
        TempData["Success"] = "Plano cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var model = await planos.ObterAsync(id, ct);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PlanoVm model, CancellationToken ct)
    {
        if (model.Id == Guid.Empty) return BadRequest();
        if (!ModelState.IsValid) return View(model);
        try { await planos.SalvarAsync(model, ct); }
        catch (HttpRequestException ex) { ModelState.AddModelError("", ex.Message); return View(model); }
        TempData["Success"] = "Plano atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspenderNotificacaoValidade(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) return BadRequest();
        await planos.SuspenderNotificacaoValidadeAsync(id, ct);
        return RedirectToAction(nameof(Index));
    }
}
