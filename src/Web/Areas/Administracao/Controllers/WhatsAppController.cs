using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.ViewModels;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao"), Authorize(Roles = "Administrador")]
public class WhatsAppController(WhatsAppApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        try
        {
            var model = await api.ObterAsync(ct);
            ViewData["Cadastrado"] = model is not null;
            return View(model ?? new ConfiguracaoWhatsAppVm());
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException && !ct.IsCancellationRequested)
        {
            ViewData["Indisponivel"] = true;
            ModelState.AddModelError("", "Não foi possível carregar a configuração. Tente novamente.");
            return View(new ConfiguracaoWhatsAppVm());
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ConfiguracaoWhatsAppVm model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        try
        {
            await api.SalvarAsync(model, ct);
            TempData["Sucesso"] = "Configuração do WhatsApp salva.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is OperationCanceledException && !ct.IsCancellationRequested)
        {
            ModelState.AddModelError("", "Não foi possível salvar. Tente novamente; se outro administrador cadastrou a configuração, recarregue a página.");
            return View(model);
        }
    }
}
