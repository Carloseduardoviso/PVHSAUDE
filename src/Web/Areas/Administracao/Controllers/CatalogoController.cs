using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Models;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class CatalogoController(IHttpClientFactory factory) : Controller
{
    private HttpClient Api() => factory.CreateClient("default");
    [HttpGet] public async Task<IActionResult> Especialidades(CancellationToken ct) => View("Especialidades", await Api().GetFromJsonAsync<List<CatalogoItemVm>>("api/especialidades", ct) ?? []);
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Especialidades(string nome, CancellationToken ct) { if (!string.IsNullOrWhiteSpace(nome)) await Api().PostAsJsonAsync("api/especialidades", new { nome, ativo = true }, ct); return RedirectToAction(nameof(Especialidades)); }
    [HttpGet] public async Task<IActionResult> Procedimentos(CancellationToken ct) => View("Procedimentos", await Api().GetFromJsonAsync<List<CatalogoItemVm>>("api/procedimentos", ct) ?? []);
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Procedimentos(string nome, CancellationToken ct) { if (!string.IsNullOrWhiteSpace(nome)) await Api().PostAsJsonAsync("api/procedimentos", new { nome, ativo = true }, ct); return RedirectToAction(nameof(Procedimentos)); }
}
