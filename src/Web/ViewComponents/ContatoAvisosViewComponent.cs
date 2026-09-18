using Microsoft.AspNetCore.Mvc;
using Web.Models;
using Web.Services;

namespace PVHSAUDE.Web.ViewComponents;

public class ContatoAvisosViewComponent(ContatoApiClient contatos) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!PVHSAUDE.Application.ViewModels.AcessoMenu.PodeAcessar(HttpContext.User, "Contato"))
            return Content(string.Empty);
        try
        {
            var mensagens = (await contatos.ListarAsync(HttpContext.RequestAborted))
                .Where(x => !x.NotificacaoSuspensa)
                .OrderByDescending(x => x.EnviadoEm)
                .ToList();
            return View(mensagens);
        }
        catch (HttpRequestException)
        {
            return Content(string.Empty);
        }
    }
}
