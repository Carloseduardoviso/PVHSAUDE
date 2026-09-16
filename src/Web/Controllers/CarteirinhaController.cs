using Microsoft.AspNetCore.Mvc;

namespace PVHSAUDE.Web.Controllers;

using global::Web.Services;
using PVHSAUDE.Domain.Enuns;

public class CarteirinhaController(PlanoApiClient planos) : Controller
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
}
