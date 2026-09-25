using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.ViewModels;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Beneficiario.Controllers;

[Area("Beneficiario"), Authorize(AuthenticationSchemes = BeneficiarioAuthentication.Scheme, Roles = "Beneficiario")]
public class MinhaAreaController(AcessoBeneficiarioApiClient acessos) : Controller
{
    public Task<IActionResult> Index(CancellationToken ct) => Pagina(nameof(Index), ct);
    public Task<IActionResult> Dados(CancellationToken ct) => Pagina(nameof(Dados), ct);
    public Task<IActionResult> Plano(CancellationToken ct) => Pagina(nameof(Plano), ct);
    public Task<IActionResult> Pagamentos(CancellationToken ct) => Pagina(nameof(Pagamentos), ct);
    public Task<IActionResult> Dependentes(CancellationToken ct) => Pagina(nameof(Dependentes), ct);
    public Task<IActionResult> Carteirinha(CancellationToken ct) => Pagina(nameof(Carteirinha), ct);

    private async Task<IActionResult> Pagina(string view, CancellationToken ct)
    {
        try
        {
            AreaBeneficiarioVm? model = await acessos.MinhaAreaAsync(ct);
            if (view == nameof(Dependentes) && model?.EhDependente == true)
                return RedirectToAction(nameof(Index));
            return model is null ? NotFound() : View(view, model);
        }
        catch (HttpRequestException)
        {
            return View("Indisponivel");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return View("Indisponivel");
        }
    }
}
