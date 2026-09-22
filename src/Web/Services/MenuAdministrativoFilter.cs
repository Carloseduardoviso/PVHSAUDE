using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PVHSAUDE.Application.ViewModels;
namespace Web.Services;

public class MenuAdministrativoFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        if (context.RouteData.Values["area"]?.ToString() != "Administracao") return;

        var controller = context.RouteData.Values["controller"]?.ToString();

        // Home is the authenticated landing page. User management retains its Administrator authorization.
        if (controller is "Dashboard" or "Usuario" or "Cep") return;

        var action = context.RouteData.Values["action"]?.ToString();
        var menu = controller == "Catalogo" ? action switch
        {
            "Especialidades" or "EditarEspecialidade" or "ExcluirEspecialidade" => "Especialidades",
            "Procedimentos" or "EditarProcedimento" or "ExcluirProcedimento" => "Procedimentos",
            _ => null
        } : controller;

        if (menu is null || !AcessoMenu.PodeAcessar(context.HttpContext.User, menu))
            context.Result = context.HttpContext.User.Identity?.IsAuthenticated == true ? new ForbidResult() : new ChallengeResult();
    }
}
