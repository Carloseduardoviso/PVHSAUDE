using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;
[Area("Administracao")]
public class DescontoController(DescontoApiClient descontos) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct) { try { return View(await descontos.ListarAsync(ct)); } catch (HttpRequestException) { return View(Array.Empty<PlanoVm>()); } }
    public IActionResult Create() => View(new PlanoVm());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PlanoVm model, CancellationToken ct) { model.Id = Guid.Empty; if (!ModelState.IsValid) return View(model); try { await descontos.SalvarAsync(model, ct); } catch (HttpRequestException ex) { ModelState.AddModelError("", ex.Message); return View(model); } TempData["Success"] = "Desconto cadastrado com sucesso."; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct) { var model = await descontos.ObterAsync(id, ct); return model is null ? NotFound() : View(model); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PlanoVm model, CancellationToken ct) { if (!ModelState.IsValid) return View(model); try { await descontos.SalvarAsync(model, ct); } catch (HttpRequestException ex) { ModelState.AddModelError("", ex.Message); return View(model); } TempData["Success"] = "Desconto atualizado com sucesso."; return RedirectToAction(nameof(Index)); }
}
