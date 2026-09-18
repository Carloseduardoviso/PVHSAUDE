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
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> EditarEspecialidade(Guid id, string nome, CancellationToken ct) { try { using var response = await Api().PutAsJsonAsync($"api/especialidades/{id}", new { nome, ativo = true }, ct); response.EnsureSuccessStatusCode(); TempData["Success"] = "Especialidade atualizada com sucesso."; } catch (HttpRequestException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Especialidades)); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> ExcluirEspecialidade(Guid id, CancellationToken ct) { try { using var response = await Api().DeleteAsync($"api/especialidades/{id}", ct); response.EnsureSuccessStatusCode(); TempData["Success"] = "Especialidade excluída com sucesso."; } catch (HttpRequestException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Especialidades)); }
    [HttpGet] public async Task<IActionResult> Procedimentos(CancellationToken ct) => View("Procedimentos", await Api().GetFromJsonAsync<List<CatalogoItemVm>>("api/procedimentos", ct) ?? []);
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Procedimentos(string nome, CancellationToken ct) { if (!string.IsNullOrWhiteSpace(nome)) await Api().PostAsJsonAsync("api/procedimentos", new { nome, ativo = true }, ct); return RedirectToAction(nameof(Procedimentos)); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> EditarProcedimento(Guid id, string nome, CancellationToken ct) { try { using var response = await Api().PutAsJsonAsync($"api/procedimentos/{id}", new { nome, ativo = true }, ct); response.EnsureSuccessStatusCode(); TempData["Success"] = "Procedimento atualizado com sucesso."; } catch (HttpRequestException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Procedimentos)); }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> ExcluirProcedimento(Guid id, CancellationToken ct) { try { using var response = await Api().DeleteAsync($"api/procedimentos/{id}", ct); response.EnsureSuccessStatusCode(); TempData["Success"] = "Procedimento excluído com sucesso."; } catch (HttpRequestException ex) { TempData["Error"] = ex.Message; } return RedirectToAction(nameof(Procedimentos)); }
}
