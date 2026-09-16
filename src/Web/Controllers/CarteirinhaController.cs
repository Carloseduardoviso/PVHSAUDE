using Microsoft.AspNetCore.Mvc;

namespace PVHSAUDE.Web.Controllers;

using global::Web.Services;
using global::Web.Models;
using PVHSAUDE.Domain.Enuns;

public class CarteirinhaController(PlanoApiClient planos, IntencaoVendaApiClient intencoes) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            ViewBag.Planos = (await planos.ListarAsync(cancellationToken))
                .Where(x => x.TipoPessoa == TipoPessoa.Fisica).ToList();
        }
        catch (HttpRequestException)
        {
            ViewBag.Planos = Array.Empty<global::Web.Models.PlanoVm>();
            ViewData["PlanosErro"] = "Não foi possível carregar os planos. Tente novamente em instantes.";
        }
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CriarIntencao([FromBody] IntencaoVendaEntradaVm vm, CancellationToken ct)
    {
        try { await intencoes.CriarAsync(vm, ct); return Ok(); }
        catch (HttpRequestException) { return StatusCode(503, new { message = "Não foi possível registrar a solicitação." }); }
    }
}
