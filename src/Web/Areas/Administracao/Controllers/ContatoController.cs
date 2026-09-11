using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;
namespace PVHSAUDE.Web.Areas.Administracao.Controllers;
[Area("Administracao"), Authorize]
public class ContatoController(ContatoApiClient api) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try { return View(await api.ListarAsync(ct)); }
        catch (HttpRequestException) { ViewData["Erro"] = "Não foi possível carregar as mensagens."; return View(new List<ContatoVm>()); }
    }
    [HttpGet] public async Task<IActionResult> Detalhes(Guid id,CancellationToken ct){var c=await api.ObterAsync(id,ct);return c is null?NotFound():View(c);}
    [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Excluir(Guid id,CancellationToken ct){try{await api.ExcluirAsync(id,ct);TempData["Sucesso"]="Mensagem excluída.";}catch(HttpRequestException){TempData["Erro"]="Não foi possível excluir a mensagem.";}return RedirectToAction(nameof(Index));}
}
