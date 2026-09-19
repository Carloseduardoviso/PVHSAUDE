using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class BeneficiarioController(BeneficiarioApiClient beneficiarios, PlanoApiClient planos, EmpresaBeneficiadaApiClient empresasBeneficiadas) : Controller
{
    public async Task<IActionResult> Index(string? nome, DateTime? inicioBeneficio, DateTime? validadeBeneficio, CancellationToken cancellationToken)
    {
        var model = await beneficiarios.ListarAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(nome))
        {
            var termo = nome.Trim();
            model = model.Where(x => x.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) || x.Dependentes.Any(d => d.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))).ToList();
        }
        if (inicioBeneficio.HasValue) model = model.Where(x => x.DataInicio.Date == inicioBeneficio.Value.Date).ToList();
        if (validadeBeneficio.HasValue) model = model.Where(x => x.DataValidade.Date == validadeBeneficio.Value.Date).ToList();
        ViewBag.Nome = nome;
        ViewBag.InicioBeneficio = inicioBeneficio?.ToString("yyyy-MM-dd");
        ViewBag.ValidadeBeneficio = validadeBeneficio?.ToString("yyyy-MM-dd");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await CarregarPlanos(cancellationToken);
        var model = new BeneficiarioVm();
        try
        {
            var beneficiariosCadastrados = await beneficiarios.ListarAsync(cancellationToken);
            var maior = beneficiariosCadastrados
                .SelectMany(x => new[] { x.Codigo }.Concat(x.Dependentes.Select(d => d.Codigo)))
                .Select(ExtrairNumero)
                .DefaultIfEmpty(0)
                .Max();
            model.Codigo = $"RO{maior + 1:000}/{DateTime.UtcNow:yyyy}";
        }
        catch (HttpRequestException)
        {
            // Mantém o valor visual padrão se a API estiver indisponível; o servidor valida a sequência ao salvar.
        }
        return View(model);
    }

    private static int ExtrairNumero(string? codigo)
    {
        var match = System.Text.RegularExpressions.Regex.Match(codigo ?? string.Empty, @"^RO(\d+)/\d{4}$");
        return match.Success && int.TryParse(match.Groups[1].Value, out var numero) ? numero : 0;
    }

    private async Task CarregarPlanos(CancellationToken ct)
    {
        ViewBag.EmpresasBeneficiadasDisponiveis = new List<CredenciadoVm>();
        ViewBag.PlanosDisponiveis = new List<PlanoVm>();
        ViewBag.EmpresasBeneficiadas = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
        try
        {
            var catalogo = await planos.ListarAsync(ct);
            var planosPessoaFisica = catalogo
                .Where(p => p.TipoPessoa == PVHSAUDE.Domain.Enuns.TipoPessoa.Fisica)
                .ToList();
            ViewBag.PlanosDisponiveis = planosPessoaFisica;
            ViewBag.Planos = planosPessoaFisica
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(p.Nome, p.Id.ToString())).ToList();
            var empresas = await empresasBeneficiadas.ListarAsync(ct);
            ViewBag.EmpresasBeneficiadasDisponiveis = empresas;
            ViewBag.EmpresasBeneficiadas = empresas
                .Select(p => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(p.NomeFantasia, p.Id.ToString())).ToList();
        }
        catch (HttpRequestException)
        {
            ViewBag.Planos = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            ModelState.AddModelError("", "Não foi possível carregar os planos e as empresas beneficiadas. Verifique a API e tente novamente.");
        }
    }

    private void DefinirPlanoEmpresa(BeneficiarioVm model)
    {
        if (model.TipoPessoa != PVHSAUDE.Domain.Enuns.TipoPessoa.Juridica)
        {
            model.EmpresaBeneficiadaId = null;
            model.CredenciadoId = null;
            return;
        }
        var empresa = ((IEnumerable<CredenciadoVm>)ViewBag.EmpresasBeneficiadasDisponiveis).FirstOrDefault(x => x.Id == model.EmpresaBeneficiadaId);
        ModelState.Remove(nameof(model.PlanoId));
        model.PlanoId = empresa?.PlanoId ?? Guid.Empty;
        model.CredenciadoId = null;
        if (model.PlanoId == Guid.Empty) ModelState.AddModelError(nameof(model.EmpresaBeneficiadaId), "Selecione uma empresa beneficiada com plano cadastrado.");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BeneficiarioVm model, CancellationToken cancellationToken)
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
    public async Task<IActionResult> Edit(BeneficiarioVm model, CancellationToken cancellationToken)
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
