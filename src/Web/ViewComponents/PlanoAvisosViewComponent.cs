using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.ViewComponents;

public class PlanoAvisosViewComponent(PlanoApiClient planos) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        try
        {
            var hoje = DateTime.Today;
            var avisos = (await planos.ListarAsync(HttpContext.RequestAborted))
                .Where(x => x.DataValidade.HasValue && !x.NotificacaoValidadeSuspensa)
                .Where(x => x.DataValidade!.Value.Date <= hoje.AddDays(1))
                .OrderBy(x => x.DataValidade)
                .ToList();
            return View(avisos);
        }
        catch (HttpRequestException)
        {
            return Content(string.Empty);
        }
    }
}
