using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
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
                var ctx = Context(controller, action, method, new ClaimsPrincipal(identity), authorize: true);
                new MenuApiFilter().OnAuthorization(ctx);
                if ((ctx.Result is null) != permitido)
                    throw new Exception($"Falha de autorização: {controller}, {role}, permitido={permitido}");
            }
        }

        foreach (var (controller, action, method, allowAnonymous) in new[]
        {
            ("Banners", "Ativos", "GET", true),
            ("Contatos", "Criar", "POST", true),
            ("Planos", "Listar", "GET", false),
            ("Banners", "Imagem", "GET", false)
        })
        {
            var ctx = Context(controller, action, method, new ClaimsPrincipal(), allowAnonymous: allowAnonymous);
            new MenuApiFilter().OnAuthorization(ctx);
            if (ctx.Result is not null) throw new Exception($"Rota pública indevidamente bloqueada: {controller}/{action}.");
        }

        var menus = (IDictionary<string, string>)MenusAdministrativos.Opcoes;
        menus["Categoria"] = "Categorias";
        try
        {
            var usuarioSemMenu = new ClaimsPrincipal(new ClaimsIdentity([], "tests"));
            var ctx = Context("Categorias", "Criar", "POST", usuarioSemMenu);
            new MenuApiFilter().OnAuthorization(ctx);
            if (ctx.Result is not ForbidResult)
                throw new Exception("Um controller novo não foi associado automaticamente ao menu singular.");

            var usuarioComMenu = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(AcessoMenu.Claim, "Categoria")], "tests"));
            ctx = Context("Categorias", "Criar", "POST", usuarioComMenu);
            new MenuApiFilter().OnAuthorization(ctx);
            if (ctx.Result is not null)
                throw new Exception("A permissão do novo menu não liberou o controller correspondente.");

            ctx = Context("Categorias", "Criar", "POST", usuarioSemMenu, allowAnonymous: true);
            new MenuApiFilter().OnAuthorization(ctx);
            if (ctx.Result is not null)
                throw new Exception("AllowAnonymous não foi respeitado para um menu dinâmico.");
        }
        finally
        {
            menus.Remove("Categoria");
        }

        var desconhecido = Context("ControllerSemMenu", "Criar", "POST", new ClaimsPrincipal());
        new MenuApiFilter().OnAuthorization(desconhecido);
        if (desconhecido.Result is not null)
            throw new Exception("Um controller sem menu cadastrado foi bloqueado.");

        if (new MenusValidosAttribute().IsValid(new[] { "Usuario" }) || new MenusValidosAttribute().IsValid(new[] { "Inexistente" }))
            throw new Exception("Menus inválidos foram aceitos.");
        Console.WriteLine("PASS: permissões dinâmicas da API, rotas públicas e menus inválidos verificados.");
        return Task.CompletedTask;
    }

    private static AuthorizationFilterContext Context(
        string controller,
        string action,
        string method,
        ClaimsPrincipal user,
        bool authorize = false,
        bool allowAnonymous = false)
    {
        var http = new DefaultHttpContext { User = user };
        http.Request.Method = method;
        var route = new RouteData();
        route.Values["controller"] = controller;
        route.Values["action"] = action;
        var metadata = new List<object>();
        if (authorize) metadata.Add(new AuthorizeAttribute());
        if (allowAnonymous) metadata.Add(new AllowAnonymousAttribute());
        var descriptor = new ActionDescriptor { EndpointMetadata = metadata };
        return new AuthorizationFilterContext(new ActionContext(http, route, descriptor, new ModelStateDictionary()), []);
    }
}
