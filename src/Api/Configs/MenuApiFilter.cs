using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Configs;

public class MenuApiFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var action = context.RouteData.Values["action"]?.ToString();
        var get = HttpMethods.IsGet(context.HttpContext.Request.Method);
        var menu = controller switch
        {
            "Beneficiarios" => "Beneficiario",
            "Banners" when action is not ("Ativos" or "Imagem") => "Banner",
            "Contatos" when action != "Criar" => "Contato",
            "Planos" when !get => "Plano",
            "Credenciados" when !get => "Credenciado",
            "Especialidades" when !get => "Especialidades",
            "Procedimentos" when !get => "Procedimentos",
            _ => null
        };
        if (menu is not null && !AcessoMenu.PodeAcessar(context.HttpContext.User, menu))
            context.Result = context.HttpContext.User.Identity?.IsAuthenticated == true ? new ForbidResult() : new ChallengeResult();
    }
}
