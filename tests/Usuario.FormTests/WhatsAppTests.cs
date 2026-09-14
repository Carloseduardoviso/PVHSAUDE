using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using Web.Services;

internal static class WhatsAppTests
{
    public static async Task Run()
    {
        void Check(bool ok, string message) { if (!ok) throw new Exception(message); Console.WriteLine("PASS: " + message); }
        var model = new ConfiguracaoWhatsAppVm { Nome = "PVH Saúde", Mensagem = "Olá! Quero informações & valores + esportes.", Telefone = "5569992341486" };
        Check(Validator.TryValidateObject(model, new ValidationContext(model), null, true), "Configuração válida aceita.");
        var link = new Uri(model.CriarLink());
        Check(link.Host == "wa.me" && link.AbsolutePath == "/5569992341486" && Uri.UnescapeDataString(link.Query[6..]) == model.Nome + "\n" + model.Mensagem, "Telefone, nome, acentos e caracteres especiais preservados no link.");
        foreach (var phone in new[] { "", "123", "55/evil", "+55 (69) 99999-9999", "00000000000" })
        {
            model.Telefone = phone;
            Check(!Validator.TryValidateObject(model, new ValidationContext(model), null, true), "Telefone inválido rejeitado: " + phone);
        }
        model.Telefone = "5569992341486";
        var options = new DbContextOptionsBuilder<Context>().UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MetadataOnly;Integrated Security=true").Options;
        using var db = new Context(options, null!);
        var entity = db.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(ConfiguracaoWhatsApp))!;
        Check(entity.FindPrimaryKey()!.Properties.Single().Name == "Id" && entity.FindProperty("Id")!.ValueGenerated == ValueGenerated.Never && entity.GetCheckConstraints().Single().Sql == "[Id] = 1", "Modelo do banco restringe todos os registros à mesma chave, sem identidade automática.");
        Check(typeof(PVHSAUDE.Api.Controllers.ConfiguracaoWhatsAppController).GetMethod("Salvar")!.GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single().Roles == "Administrador", "Gravação na API restrita a administrador.");
        Check(typeof(PVHSAUDE.Web.Areas.Administracao.Controllers.WhatsAppController).GetCustomAttributes(typeof(AuthorizeAttribute), true).Cast<AuthorizeAttribute>().Single().Roles == "Administrador", "Tela administrativa protegida.");
        var transport = new Transport(model);
        var api = new WhatsAppApiClient(new Factory(transport));
        var controller = new PVHSAUDE.Web.Controllers.WhatsAppController(api) { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() } };
        Check(await controller.Index(default) is RedirectResult redirect && redirect.Url == model.CriarLink(), "Link público redireciona para a configuração cadastrada.");
        model.Mensagem = "Mensagem editada";
        Check(await controller.Index(default) is RedirectResult updated && updated.Url.Contains(Uri.EscapeDataString(model.Mensagem)), "Edição refletida no próximo clique.");
        transport.Missing = true;
        Check(await controller.Index(default) is ViewResult && controller.Response.StatusCode == 503, "Sem cadastro, não redireciona para telefone fixo.");
    }

    private sealed class Factory(HttpMessageHandler handler) : IHttpClientFactory
    { public HttpClient CreateClient(string name) => new(handler, false) { BaseAddress = new Uri("http://test-api/") }; }

    private sealed class Transport(ConfiguracaoWhatsAppVm model) : HttpMessageHandler
    {
        public bool Missing;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct) => Task.FromResult(Missing
            ? new HttpResponseMessage(HttpStatusCode.NotFound)
            : new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(model) });
    }
}
