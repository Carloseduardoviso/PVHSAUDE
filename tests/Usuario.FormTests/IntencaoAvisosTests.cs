using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Web.Models;
using Web.Services;
using PVHSAUDE.Web.ViewComponents;

public static class IntencaoAvisosTests
{
    public static async Task Run()
    {
        var intencoes = new[]
        {
            new IntencaoVendaVm { Id = Guid.NewGuid(), Nome = "Ana", PlanoNome = "Plano teste" },
            new IntencaoVendaVm { Id = Guid.NewGuid(), Nome = "Bruno", PlanoNome = "Plano suspenso", NotificacaoSuspensa = true }
        };
        using var client = new HttpClient(new Handler(intencoes)) { BaseAddress = new Uri("http://api/") };
        var component = new IntencaoAvisosViewComponent(new IntencaoVendaApiClient(client))
        {
            ViewComponentContext = new ViewComponentContext { ViewContext = new ViewContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(PVHSAUDE.Application.ViewModels.AcessoMenu.Claim, "IntencaoVenda")], "tests")) } } }
        };
        var result = await component.InvokeAsync();
        if (result is not ViewViewComponentResult { ViewData.Model: IReadOnlyCollection<IntencaoVendaVm> model } || model.Count != 1 || model.Any(x => x.NotificacaoSuspensa))
            throw new Exception("A notificação de intenções deve ocultar intenções suspensas.");
        Console.WriteLine("PASS: notificação de intenções retorna somente intenções ativas.");
    }

    private sealed class Handler(IntencaoVendaVm[] intencoes) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(intencoes) });
    }
}
