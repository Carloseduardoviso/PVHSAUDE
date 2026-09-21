using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Services;
namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao"), Authorize]
public class LogoPortalController(LogoPortalStorage logos) : Controller
{
    [HttpGet] public IActionResult Index() => View(logos.Obter() is not null);
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(IFormFile? logo, CancellationToken ct)
    {
        if (logo is null) { ModelState.AddModelError("logo", "Selecione uma imagem."); return View(logos.Obter() is not null); }
        try { await logos.SalvarAsync(logo, ct); TempData["Sucesso"] = "Logo do portal atualizada."; return RedirectToAction(nameof(Index)); }
        catch (InvalidOperationException ex) { ModelState.AddModelError("logo", ex.Message); return View(logos.Obter() is not null); }
    }
}
