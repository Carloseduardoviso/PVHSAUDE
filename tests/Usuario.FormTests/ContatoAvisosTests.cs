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

public static class ContatoAvisosTests
{
    public static async Task Run()
    {
        var contatos = new[]
        {
            new ContatoVm { Id = Guid.NewGuid(), Nome = "Ana", Email = "ana@example.com", Telefone = "9999", Mensagem = "Uma mensagem" },
            new ContatoVm { Id = Guid.NewGuid(), Nome = "Bruno", Email = "bruno@example.com", Telefone = "8888", Mensagem = "Outra mensagem", NotificacaoSuspensa = true }
        };
        using var client = new HttpClient(new Handler(contatos)) { BaseAddress = new Uri("http://api/") };
        var component = new ContatoAvisosViewComponent(new ContatoApiClient(client))
        {
            ViewComponentContext = new ViewComponentContext { ViewContext = new ViewContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(PVHSAUDE.Application.ViewModels.AcessoMenu.Claim, "Contato")], "tests")) } } }
        };

        var result = await component.InvokeAsync();
        if (result is not ViewViewComponentResult { ViewData.Model: IReadOnlyCollection<ContatoVm> model } || model.Count != 1 || model.Any(x => x.NotificacaoSuspensa))
            throw new Exception("A notificação de mensagens deve ocultar contatos suspensos.");
        Console.WriteLine("PASS: notificação de mensagens retorna a quantidade de contatos recebidos.");
    }

    private sealed class Handler(ContatoVm[] contatos) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(contatos) });
    }
}
