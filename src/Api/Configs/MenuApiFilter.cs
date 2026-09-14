using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Api.Configs;

public class MenuApiFilter : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var controller = context.RouteData.Values["controller"]?.ToString();
        var menu = EncontrarMenu(controller);
        if (menu is null) return;

        var metadata = context.ActionDescriptor.EndpointMetadata;
        if (metadata.OfType<IAllowAnonymous>().Any()) return;

        var protegido = !HttpMethods.IsGet(context.HttpContext.Request.Method)
            || metadata.OfType<IAuthorizeData>().Any();
        if (protegido && !AcessoMenu.PodeAcessar(context.HttpContext.User, menu))
            context.Result = context.HttpContext.User.Identity?.IsAuthenticated == true
                ? new ForbidResult()
                : new ChallengeResult();
    }

    private static string? EncontrarMenu(string? controller)
    {
        if (string.IsNullOrWhiteSpace(controller)) return null;

        var menu = MenusAdministrativos.Opcoes.Keys.FirstOrDefault(opcao =>
            string.Equals(opcao, controller, StringComparison.OrdinalIgnoreCase));
        if (menu is not null) return menu;

        var singular = controller.EndsWith('s') ? controller[..^1] : controller;
        return MenusAdministrativos.Opcoes.Keys.FirstOrDefault(opcao =>
            string.Equals(opcao, singular, StringComparison.OrdinalIgnoreCase));
    }
}
