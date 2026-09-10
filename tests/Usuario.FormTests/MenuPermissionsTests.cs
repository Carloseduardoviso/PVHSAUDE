using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using PVHSAUDE.Api.Configs;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Enuns;

public static class MenuPermissionsTests
{
    public static Task Run()
    {
        foreach (var (controller, action, method, menu) in new[]
        {
            ("Beneficiarios", "Listar", "GET", "Beneficiario"),
            ("Banners", "Listar", "GET", "Banner"),
            ("Banners", "Editar", "PUT", "Banner"),
            ("Contatos", "Listar", "GET", "Contato"),
            ("Contatos", "Excluir", "DELETE", "Contato"),
            ("Planos", "Criar", "POST", "Plano"),
            ("Credenciados", "UploadImagem", "POST", "Credenciado"),
            ("Especialidades", "Post", "POST", "Especialidades"),
            ("Procedimentos", "Post", "POST", "Procedimentos")
        })
        {
            foreach (var role in new[] { "Comum", "Gestor", "Administrador" })
            foreach (var permitido in new[] { false, true })
            {
                var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "tests");
                if (permitido) identity.AddClaim(new Claim(AcessoMenu.Claim, menu));
                var ctx = Context(controller, action, method, new ClaimsPrincipal(identity));
                new MenuApiFilter().OnAuthorization(ctx);
                if ((ctx.Result is null) != permitido)
                    throw new Exception($"Falha de autorização: {controller}, {role}, permitido={permitido}");
            }
        }
        foreach (var (controller, action, method) in new[] { ("Banners", "Ativos", "GET"), ("Contatos", "Criar", "POST"), ("Planos", "Listar", "GET") })
        {
            var ctx = Context(controller, action, method, new ClaimsPrincipal());
            new MenuApiFilter().OnAuthorization(ctx);
            if (ctx.Result is not null) throw new Exception("Consulta pública indevidamente bloqueada.");
        }
        if (new MenusValidosAttribute().IsValid(new[] { "Usuario" }) || new MenusValidosAttribute().IsValid(new[] { "Inexistente" }))
            throw new Exception("Menus inválidos foram aceitos.");
        Console.WriteLine("PASS: permissões da API verificadas para Comum, Gestor e Administrador; portal público preservado; menus inválidos rejeitados.");
        return Task.CompletedTask;
    }
    private static AuthorizationFilterContext Context(string controller, string action, string method, ClaimsPrincipal user)
    {
        var http = new DefaultHttpContext { User = user };
        http.Request.Method = method;
        var route = new RouteData();
        route.Values["controller"] = controller;
        route.Values["action"] = action;
        return new AuthorizationFilterContext(new ActionContext(http, route, new ActionDescriptor(), new ModelStateDictionary()), []);
    }
}
