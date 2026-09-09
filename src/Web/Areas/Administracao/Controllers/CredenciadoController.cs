using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class CredenciadoController(CredenciadoApiClient credenciados, PlanoApiClient planos) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try { return View(await credenciados.ListarAsync(ct)); }
        catch (HttpRequestException) { ViewData["Error"] = "Não foi possível carregar os credenciados. Verifique se a API está disponível."; return View(Array.Empty<CredenciadoViewModel>()); }
    }

    [HttpGet]
    private async Task Catalogos(CancellationToken ct) { ViewBag.Planos = (await planos.ListarAsync(ct)).Select(x => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(x.Nome + " — " + x.Valor.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")), x.Id.ToString())).ToList(); ViewBag.Especialidades = await credenciados.EspecialidadesAsync(ct); ViewBag.Procedimentos = await credenciados.ProcedimentosAsync(ct); }
    public async Task<IActionResult> Create(CancellationToken ct) { await Catalogos(ct); return View(new CredenciadoViewModel()); }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var model = await credenciados.ObterAsync(id, ct);
        await Catalogos(ct);
        if (model is null) return NotFound(); await Catalogos(ct); return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CredenciadoViewModel model, CancellationToken ct)
    {
        model.Id = Guid.Empty;
        await Catalogos(ct);
        if (!ModelState.IsValid) return View(model);
        try { await credenciados.SalvarAsync(model, ct);
        if (model.Imagem is not null) await credenciados.UploadImagemAsync(model.Id, model.Imagem, ct);
        if (model.Imagem is not null)
        {
            var lista = await credenciados.ListarAsync(ct);
            var cnpj = new string(model.Cnpj.Where(char.IsDigit).ToArray());
            var salvo = lista.FirstOrDefault(x => x.Cnpj == cnpj);
            if (salvo is not null) await credenciados.UploadImagemAsync(salvo.Id, model.Imagem, ct);
        } }
        catch (HttpRequestException ex) { ModelState.AddModelError("", ex.StatusCode == System.Net.HttpStatusCode.Conflict ? "Já existe uma empresa credenciada com este CNPJ." : string.IsNullOrWhiteSpace(ex.Message) ? "Não foi possível salvar a empresa. Verifique os dados e a conexão com a API." : ex.Message); return View(model); }
        TempData["Success"] = "Empresa credenciada cadastrada com sucesso.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var model = await credenciados.ObterAsync(id, ct);
        await Catalogos(ct);
        if (model is null) return NotFound(); await Catalogos(ct); return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CredenciadoViewModel model, CancellationToken ct)
    {
        if (model.Id == Guid.Empty) return BadRequest();
        await Catalogos(ct);
        if (!ModelState.IsValid) return View(model);
        try { await credenciados.SalvarAsync(model, ct);
        if (model.Imagem is not null) await credenciados.UploadImagemAsync(model.Id, model.Imagem, ct);
        if (model.Imagem is not null)
        {
            var lista = await credenciados.ListarAsync(ct);
            var cnpj = new string(model.Cnpj.Where(char.IsDigit).ToArray());
            var salvo = lista.FirstOrDefault(x => x.Cnpj == cnpj);
            if (salvo is not null) await credenciados.UploadImagemAsync(salvo.Id, model.Imagem, ct);
        } }
        catch (HttpRequestException ex) { ModelState.AddModelError("", ex.StatusCode == System.Net.HttpStatusCode.Conflict ? "Já existe uma empresa credenciada com este CNPJ." : string.IsNullOrWhiteSpace(ex.Message) ? "Não foi possível salvar a empresa. Verifique os dados e a conexão com a API." : ex.Message); return View(model); }
        TempData["Success"] = "Empresa credenciada atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
