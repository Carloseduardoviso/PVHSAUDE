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
        if (controller is "Dashboard" or "Usuario") return;

        var menu = controller == "Catalogo" ? context.RouteData.Values["action"]?.ToString() : controller;

        if (menu is null || !AcessoMenu.PodeAcessar(context.HttpContext.User, menu))
            context.Result = context.HttpContext.User.Identity?.IsAuthenticated == true ? new ForbidResult() : new ChallengeResult();
    }
}
