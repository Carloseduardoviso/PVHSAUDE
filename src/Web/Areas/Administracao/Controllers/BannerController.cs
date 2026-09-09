using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;
namespace PVHSAUDE.Web.Areas.Administracao.Controllers;
[Area("Administracao"), Authorize]
public class BannerController(BannerApiClient banners) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try { return View(await banners.ListarAsync(false, ct)); }
        catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException && !ct.IsCancellationRequested)
        { ViewData["Erro"] = "Não foi possível carregar os banners. Tente novamente."; return View(new List<BannerViewModel>()); }
    }
    [HttpGet] public IActionResult Create() => View("Form", new BannerViewModel());
    [HttpGet] public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        try { var b = await banners.ObterAsync(id, ct); return b is null ? NotFound() : View("Form", b); }
        catch (HttpRequestException) { TempData["Error"] = "Não foi possível carregar o banner."; return RedirectToAction(nameof(Index)); }
    }
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6_291_456)]
    public Task<IActionResult> Create(BannerViewModel model, CancellationToken ct) { model.Id = Guid.Empty; return Salvar(model, ct); }
    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(6_291_456)]
    public Task<IActionResult> Edit(Guid id, BannerViewModel model, CancellationToken ct)
    { if (id != model.Id) return Task.FromResult<IActionResult>(BadRequest()); return Salvar(model, ct); }
    private async Task<IActionResult> Salvar(BannerViewModel model, CancellationToken ct)
    {
        if (model.Id == Guid.Empty && model.Imagem is null) ModelState.AddModelError("Imagem", "Selecione uma imagem.");
        if (model.Imagem is { } imagem && (imagem.Length == 0 || imagem.Length > 5_242_880)) ModelState.AddModelError("Imagem", "Envie uma imagem de até 5 MB.");
        if (!ModelState.IsValid) return View("Form", model);
        try
        {
            var erro = await banners.SalvarAsync(model, ct);
            if (erro is null) { TempData["Success"] = "Banner salvo com sucesso."; return RedirectToAction(nameof(Index)); }
            ModelState.AddModelError("", erro);
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException && !ct.IsCancellationRequested)
        { ModelState.AddModelError("", "Não foi possível salvar o banner. Tente novamente."); }
        return View("Form", model);
    }
}
