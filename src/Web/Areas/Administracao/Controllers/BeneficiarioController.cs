using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class BeneficiarioController(IBeneficiarioApiClient beneficiarios) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await beneficiarios.ListarAsync(cancellationToken));

    [HttpGet]
    public IActionResult Create() => View(new BeneficiarioViewModel());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BeneficiarioViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try { await beneficiarios.CriarAsync(model, cancellationToken); }
        catch (HttpRequestException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(model); }
        TempData["Success"] = "Beneficiário cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var model = await beneficiarios.ObterAsync(id, cancellationToken);
        return model is null ? NotFound() : View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BeneficiarioViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        try { await beneficiarios.AtualizarAsync(model, cancellationToken); }
        catch (HttpRequestException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(model); }
        TempData["Success"] = "Cadastro atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try { await beneficiarios.ExcluirAsync(id, cancellationToken); }
        catch (HttpRequestException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}
