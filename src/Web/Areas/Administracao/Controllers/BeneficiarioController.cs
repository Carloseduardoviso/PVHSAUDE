using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class BeneficiarioController(IBeneficiarioApiClient beneficiarios, PlanoApiClient planos, CredenciadoApiClient credenciados) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await beneficiarios.ListarAsync(cancellationToken));

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await CarregarPlanos(cancellationToken);
        return View(new BeneficiarioViewModel());
    }

    private async Task CarregarPlanos(CancellationToken ct)
    {
        ViewBag.EmpresasDisponiveis = new List<CredenciadoViewModel>();
        ViewBag.PlanosDisponiveis = new List<PlanoViewModel>();
        ViewBag.Empresas = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
        try
        {
            var catalogo = await planos.ListarAsync(ct);
            ViewBag.PlanosDisponiveis = catalogo;
            ViewBag.Planos = catalogo
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(p.Nome, p.Id.ToString())).ToList();
            var empresas = await credenciados.ListarAsync(ct);
            ViewBag.EmpresasDisponiveis = empresas;
            ViewBag.Empresas = empresas
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(p.NomeFantasia, p.Id.ToString())).ToList();
        }
        catch (HttpRequestException)
        {
            ViewBag.Planos = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            ModelState.AddModelError("", "Não foi possível carregar os planos e as empresas. Verifique a API e tente novamente.");
        }
    }

    private void DefinirPlanoEmpresa(BeneficiarioViewModel model)
    {
        var empresa = ((IEnumerable<CredenciadoViewModel>)ViewBag.EmpresasDisponiveis).FirstOrDefault(x => x.Id == model.CredenciadoId);
        ModelState.Remove(nameof(model.PlanoId));
        model.PlanoId = empresa?.PlanoId ?? Guid.Empty;
        if (model.PlanoId == Guid.Empty) ModelState.AddModelError(nameof(model.CredenciadoId), "Selecione uma empresa com plano cadastrado.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BeneficiarioViewModel model, CancellationToken cancellationToken)
    {
        await CarregarPlanos(cancellationToken);
        DefinirPlanoEmpresa(model);
        if (!ModelState.IsValid) return View(model);
        try { await beneficiarios.CriarAsync(model, cancellationToken); }
        catch (HttpRequestException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(model); }
        TempData["Success"] = "Beneficiário cadastrado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken)
    {
        var model = await beneficiarios.ObterAsync(id, cancellationToken);
        if (model is null) return NotFound();
        await CarregarPlanos(cancellationToken);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BeneficiarioViewModel model, CancellationToken cancellationToken)
    {
        await CarregarPlanos(cancellationToken);
        DefinirPlanoEmpresa(model);
        if (!ModelState.IsValid) return View(model);
        try { await beneficiarios.AtualizarAsync(model, cancellationToken); }
        catch (HttpRequestException ex) { ModelState.AddModelError(string.Empty, ex.Message); return View(model); }
        TempData["Success"] = "Cadastro atualizado com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Inativar(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await beneficiarios.InativarAsync(id, cancellationToken);
            TempData["Success"] = "Beneficiário inativado com sucesso.";
        }
        catch (HttpRequestException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
    {
        var model = await beneficiarios.ObterAsync(id, cancellationToken);
        if (model is null) return NotFound();
        await CarregarPlanos(cancellationToken);
        return View(model);
    }
}
