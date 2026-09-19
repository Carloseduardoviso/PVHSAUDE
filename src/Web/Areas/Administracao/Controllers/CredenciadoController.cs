using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class CredenciadoController(CredenciadoApiClient credenciados, DescontoApiClient descontos) : Controller
{
    public async Task<IActionResult> Index(string? nomeFantasia, DateTime? dataCadastro, CancellationToken ct)
    {
        try
        {
            var model = await credenciados.ListarAsync(ct);
            if (!string.IsNullOrWhiteSpace(nomeFantasia)) model = model.Where(x => x.NomeFantasia.Contains(nomeFantasia.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            if (dataCadastro.HasValue) model = model.Where(x => x.DataCadastro.ToLocalTime().Date == dataCadastro.Value.Date).ToList();
            ViewBag.NomeFantasia = nomeFantasia;
            ViewBag.DataCadastro = dataCadastro?.ToString("yyyy-MM-dd");
            return View(model);
        }
        catch (HttpRequestException) { ViewData["Error"] = "Não foi possível carregar os credenciados."; return View(Array.Empty<CredenciadoVm>()); }
    }
    private async Task Catalogos(CancellationToken ct)
    {
        ViewBag.Descontos = (await descontos.ListarAsync(ct)).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.Nome + " - " + x.Valor.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")), x.Id.ToString())).ToList();
        ViewBag.Especialidades = await credenciados.EspecialidadesAsync(ct);
        ViewBag.Procedimentos = await credenciados.ProcedimentosAsync(ct);
    }
    public async Task<IActionResult> Create(CancellationToken ct) { await Catalogos(ct); return View(new CredenciadoVm()); }
    public async Task<IActionResult> Details(Guid id, CancellationToken ct) { var model = await credenciados.ObterAsync(id, ct); await Catalogos(ct); return model is null ? NotFound() : View(model); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CredenciadoVm model, CancellationToken ct) { model.Id = Guid.Empty; await Catalogos(ct); if (!ModelState.IsValid) return View(model); try { await credenciados.SalvarAsync(model, ct); await EnviarImagem(model, ct); } catch (HttpRequestException ex) { ModelState.AddModelError("", ex.Message); return View(model); } TempData["Success"] = "Credenciamento cadastrado com sucesso."; return RedirectToAction(nameof(Index)); }
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct) { var model = await credenciados.ObterAsync(id, ct); await Catalogos(ct); return model is null ? NotFound() : View(model); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CredenciadoVm model, CancellationToken ct) { if (model.Id == Guid.Empty) return BadRequest(); await Catalogos(ct); if (!ModelState.IsValid) return View(model); try { await credenciados.SalvarAsync(model, ct); await EnviarImagem(model, ct); } catch (HttpRequestException ex) { ModelState.AddModelError("", ex.Message); return View(model); } TempData["Success"] = "Credenciamento atualizado com sucesso."; return RedirectToAction(nameof(Index)); }

    private async Task EnviarImagem(CredenciadoVm model, CancellationToken ct)
    {
        if (model.Imagens.Count == 0 && model.Imagem is not null) model.Imagens = [model.Imagem];
        if (model.Imagens.Count == 0) return;
        var id = model.Id;
        if (id == Guid.Empty)
        {
            var cnpj = new string(model.Cnpj.Where(char.IsDigit).ToArray());
            id = (await credenciados.ListarAsync(ct)).FirstOrDefault(x => x.Cnpj == cnpj)?.Id ?? Guid.Empty;
        }
        if (id != Guid.Empty)
            foreach (var imagem in model.Imagens) await credenciados.UploadImagemAsync(id, imagem, ct);
    }
}
