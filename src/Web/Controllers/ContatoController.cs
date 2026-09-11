using Microsoft.AspNetCore.Mvc; using Web.Models; using Web.Services;
namespace PVHSAUDE.Web.Controllers; public class ContatoController(ContatoApiClient api):Controller{
[HttpGet] public IActionResult Index()=>View(new ContatoVm());
[HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Index(ContatoVm m,CancellationToken ct){if(!ModelState.IsValid)return View(m);try{await api.EnviarAsync(m,ct);TempData["Sucesso"]="Mensagem enviada com sucesso.";return RedirectToAction(nameof(Index));}catch(HttpRequestException){ModelState.AddModelError("","Não foi possível enviar a mensagem. Tente novamente.");return View(m);}}
}

