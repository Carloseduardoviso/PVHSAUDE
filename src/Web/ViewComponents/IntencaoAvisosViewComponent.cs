using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.ViewComponents;

public class IntencaoAvisosViewComponent(IntencaoVendaApiClient intencoes) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!PVHSAUDE.Application.ViewModels.AcessoMenu.PodeAcessar(HttpContext.User, "IntencaoVenda"))
            return Content(string.Empty);
        try
        {
            var itens = (await intencoes.ListarAsync(HttpContext.RequestAborted))
                .Where(x => !x.NotificacaoSuspensa)
                .OrderByDescending(x => x.CriadoEm)
                .ToList();
            return View(itens);
        }
        catch (HttpRequestException)
        {
            return Content(string.Empty);
        }
    }
}
