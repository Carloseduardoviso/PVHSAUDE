using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class EmpresaBeneficiadaController(EmpresaBeneficiadaApiClient empresas, PlanoApiClient planos) : Controller
{
    public async Task<IActionResult> Index(string? nomeFantasia, DateTime? dataCadastro, CancellationToken ct)
    {
        try
        {
            var model = await empresas.ListarAsync(ct);
            if (!string.IsNullOrWhiteSpace(nomeFantasia)) model = model.Where(x => x.NomeFantasia.Contains(nomeFantasia.Trim(), StringComparison.OrdinalIgnoreCase)).ToList();
            if (dataCadastro.HasValue) model = model.Where(x => x.DataCadastro.ToLocalTime().Date == dataCadastro.Value.Date).ToList();
            ViewBag.NomeFantasia = nomeFantasia;
            ViewBag.DataCadastro = dataCadastro?.ToString("yyyy-MM-dd");
            return View(model);
        }
        catch (HttpRequestException)
        {
            ViewData["Error"] = "Não foi possível carregar as empresas beneficiadas. Verifique se a API está disponível.";
            return View(Array.Empty<CredenciadoVm>());
        }
    }

    private async Task Catalogos(CancellationToken ct, Guid? planoSelecionado = null)
    {
        ViewBag.Planos = (await planos.ListarAsync(ct)).Where(x => x.TipoPessoa == PVHSAUDE.Domain.Enuns.TipoPessoa.Juridica || x.Id == planoSelecionado).Select(x => new SelectListItem(
            x.Nome + " — " + x.Valor.ToString("C", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")), x.Id.ToString())).ToList();
        ViewBag.Especialidades = await empresas.EspecialidadesAsync(ct);
        ViewBag.Procedimentos = await empresas.ProcedimentosAsync(ct);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        await Catalogos(ct);
        var model = new CredenciadoVm();
        var primeiroPlano = (ViewBag.Planos as IEnumerable<SelectListItem>)?.FirstOrDefault();
        if (Guid.TryParse(primeiroPlano?.Value, out var planoId)) model.PlanoId = planoId;
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Details(Guid id, CancellationToken ct)
    {
        var model = await empresas.ObterAsync(id, ct);
        if (model is null) return NotFound();
        await Catalogos(ct, model.PlanoId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var model = await empresas.ObterAsync(id, ct);
        if (model is null) return NotFound();
        await Catalogos(ct, model.PlanoId);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CredenciadoVm model, CancellationToken ct)
    {
        model.Id = Guid.Empty;
        return await Salvar(model, true, ct);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CredenciadoVm model, CancellationToken ct)
    {
        if (model.Id == Guid.Empty) return BadRequest();
        return await Salvar(model, false, ct);
    }

    private async Task<IActionResult> Salvar(CredenciadoVm model, bool novo, CancellationToken ct)
    {
        await Catalogos(ct);
        if (model.Imagem is { } imagem && (imagem.Length == 0 || imagem.Length > 5_242_880 ||
            !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(Path.GetExtension(imagem.FileName).ToLowerInvariant())))
            ModelState.AddModelError(nameof(model.Imagem), "Envie uma imagem JPG, PNG ou WEBP de até 5 MB.");
        if (!ModelState.IsValid) return View(novo ? "Create" : "Edit", model);
        try { await empresas.SalvarAsync(model, ct); }
        catch (HttpRequestException ex)
        {
            ModelState.AddModelError("", ex.StatusCode == System.Net.HttpStatusCode.Conflict
                ? "Já existe uma empresa beneficiada com este CNPJ."
                : "Não foi possível salvar a empresa beneficiada. Verifique os dados e a conexão com a API.");
            return View(novo ? "Create" : "Edit", model);
        }
        if (model.Imagem is not null)
        {
            try { await empresas.UploadImagemAsync(model.Id, model.Imagem, ct); }
            catch (HttpRequestException)
            {
                TempData["Error"] = "A empresa foi salva, mas a imagem não foi enviada. Selecione a imagem novamente e salve.";
                return RedirectToAction(nameof(Edit), new { id = model.Id });
            }
        }
        TempData["Success"] = novo ? "Empresa beneficiada cadastrada com sucesso." : "Empresa beneficiada atualizada com sucesso.";
        return RedirectToAction(nameof(Index));
    }
}
