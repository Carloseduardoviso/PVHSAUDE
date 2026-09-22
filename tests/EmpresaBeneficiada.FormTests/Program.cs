using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using PVHSAUDE.Web.Areas.Administracao.Controllers;
using Web.Models;
using Web.Services;

await MenuPermissionsTests.Run();
await DatabaseTests.Run();
var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName = typeof(EmpresaBeneficiadaController).Assembly.FullName });
builder.Logging.ClearProviders();
builder.Logging.AddConsole().SetMinimumLevel(LogLevel.Error);
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(EmpresaBeneficiadaController).Assembly);
var transport = new Transport();
var api = new HttpClient(transport) { BaseAddress = new Uri("http://test-api/") };
builder.Services.AddSingleton(new EmpresaBeneficiadaApiClient(api));
builder.Services.AddSingleton(new PlanoApiClient(api));
builder.Services.AddSingleton(new ContatoApiClient(api));
builder.Services.AddSingleton(new IntencaoVendaApiClient(api));
await using var app = builder.Build();
app.Use(async (ctx, next) =>
{
    ctx.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("menu", "EmpresaBeneficiada")], "test"));
    await next();
});
app.MapAreaControllerRoute("admin", "Administracao", "Administracao/{controller=Dashboard}/{action=Index}/{id?}");
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(app.Urls.Single()) };

foreach (var scenario in args.Contains("--excluir") ? Array.Empty<string>() : new[] { "valid", "cnpj", "plano", "conflict", "image", "image-failure", "edit" })
{
    var edit = scenario == "edit";
    var path = edit ? $"/Administracao/EmpresaBeneficiada/Edit/{Transport.Id}" : "/Administracao/EmpresaBeneficiada/Create";
    var html = await client.GetStringAsync(path);
    Check(html.Contains("/Administracao/EmpresaBeneficiada") && WebUtility.HtmlDecode(html).Contains("Empresas Beneficiadas"), "Menu e formulário renderizados");
    var token = WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    Check(token.Length > 0, "Token antifalsificação renderizado");
    using var form = new MultipartFormDataContent();
    foreach (var (key, value) in new Dictionary<string, string>
    {
        ["__RequestVerificationToken"] = token, ["Id"] = Transport.Id.ToString(),
        ["RazaoSocial"] = "Empresa Teste", ["NomeFantasia"] = "Beneficiada Teste",
        ["Cnpj"] = scenario == "cnpj" ? "123" : "12.345.678/0001-90",
        ["PlanoId"] = scenario == "plano" ? "" : Transport.PlanoId.ToString(),
        ["StatusCredenciamento"] = "1",
        ["EspecialidadeIds"] = Transport.EspecialidadeId.ToString(),
        ["ProcedimentoIds"] = Transport.ProcedimentoId.ToString()
    }) form.Add(new StringContent(value), key);
    if (scenario.StartsWith("image"))
    {
        var image = new ByteArrayContent(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+/ZfoAAAAASUVORK5CYII="));
        image.Headers.ContentType = new("image/png");
        form.Add(image, "Imagem", "empresa.png");
    }
    transport.Saved = null;
    transport.Uploads = 0;
    transport.Conflict = scenario == "conflict";
    transport.FailImage = scenario == "image-failure";
    using var response = await client.PostAsync(path, form);
    if (scenario is "cnpj" or "plano" or "conflict")
    {
        var body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        Check(response.StatusCode == HttpStatusCode.OK && body.Contains("Beneficiada Teste"), "Erro preserva os dados do formulário");
        if (scenario != "conflict") Check(transport.Saved is null, "Dados inválidos não são enviados à API");
        else Check(body.Contains("Já existe uma empresa beneficiada"), "CNPJ duplicado exibe mensagem");
    }
    else
    {
        Check(response.StatusCode == HttpStatusCode.Redirect, $"Cadastro/edição: {scenario}; HTTP {(int)response.StatusCode}");
        Check(transport.Saved?.PlanoId == Transport.PlanoId && transport.Saved.Tipo == PVHSAUDE.Domain.Enuns.TipoCredenciado.EmpresaBeneficiada &&
            transport.Saved.EspecialidadeIds.Contains(Transport.EspecialidadeId)
            && transport.Saved.ProcedimentoIds.Contains(Transport.ProcedimentoId), "Plano e catálogos enviados");
        Check(transport.Edited == edit, "Criação ignora ID enviado pelo navegador; edição mantém ID na rota");
        Check(transport.Uploads == (scenario.StartsWith("image") ? 1 : 0), "Imagem enviada uma única vez com ID retornado pela API");
        if (scenario == "image-failure")
        {
            Check(response.Headers.Location!.ToString().Contains("Edit"), "Falha de imagem permite tentar novamente na edição sem duplicar cadastro");
            var body = WebUtility.HtmlDecode(await client.GetStringAsync(response.Headers.Location));
            Check(body.Contains("A empresa foi salva, mas a imagem"), "Falha parcial informa que o cadastro foi salvo");
        }
    }
}
foreach (var action in new[] { "Index", $"Details/{Transport.Id}", $"Edit/{Transport.Id}" })
    Check((await client.GetStringAsync("/Administracao/EmpresaBeneficiada/" + action)).Contains("Beneficiada Teste"), "Listagem, detalhes e edição exibem a empresa");
var listHtml = await client.GetStringAsync("/Administracao/EmpresaBeneficiada/Index");
Check(listHtml.Contains("Excluir") && listHtml.Contains("Editar"), "Listagem mantém edição e oferece exclusão");
var deleteToken = WebUtility.HtmlDecode(Regex.Match(listHtml, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
using (var deleteResponse = await client.PostAsync($"/Administracao/EmpresaBeneficiada/Excluir/{Transport.Id}",
    new FormUrlEncodedContent(new Dictionary<string, string> { ["__RequestVerificationToken"] = deleteToken })))
    Check(deleteResponse.StatusCode == HttpStatusCode.Redirect && transport.Deleted, "Excluir envia DELETE à API e redireciona");
if (args.Contains("--excluir")) { await app.StopAsync(); return; }
using (var missing = await client.GetAsync($"/Administracao/EmpresaBeneficiada/Edit/{Guid.NewGuid()}"))
    Check(missing.StatusCode == HttpStatusCode.NotFound, "Empresa inexistente retorna 404");
using (var noToken = await client.PostAsync("/Administracao/EmpresaBeneficiada/Create", new FormUrlEncodedContent([])))
    Check(noToken.StatusCode == HttpStatusCode.BadRequest, "POST sem token é bloqueado");
await app.StopAsync();

static void Check(bool ok, string message) { if (!ok) throw new Exception(message); Console.WriteLine("PASS: " + message); }

sealed class Transport : HttpMessageHandler
{
    public static readonly Guid Id = Guid.NewGuid(), PlanoId = Guid.NewGuid(), EspecialidadeId = Guid.NewGuid(), ProcedimentoId = Guid.NewGuid();
    public PVHSAUDE.Application.ViewModels.EmpresaBeneficiadaEntradaVm? Saved;
    public bool Edited;
    public int Uploads;
    public bool Conflict, FailImage, Deleted;
    private static CredenciadoVm Empresa => new() { Id = Id, RazaoSocial = "Empresa Teste", NomeFantasia = "Beneficiada Teste", Cnpj = "12345678000190", PlanoId = PlanoId, Tipo = PVHSAUDE.Domain.Enuns.TipoCredenciado.EmpresaBeneficiada };
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var path = request.RequestUri!.AbsolutePath;
        if (path == "/api/planos") return Json(new[] { new PlanoVm { Id = PlanoId, Nome = "Plano teste", Valor = 99 } });
        if (path == "/api/especialidades") return Json(new[] { new CatalogoItemVm(EspecialidadeId, "Especialidade teste") });
        if (path == "/api/procedimentos") return Json(new[] { new CatalogoItemVm(ProcedimentoId, "Procedimento teste") });
        if (request.Method == HttpMethod.Get)
        {
            if (path == "/api/empresas-beneficiadas") return Json(new[] { Empresa });
            if (path == $"/api/empresas-beneficiadas/{Id}") return Json(Empresa);
            if (path.StartsWith("/api/empresas-beneficiadas/")) return new(HttpStatusCode.NotFound);
        }
        if (path == $"/api/empresas-beneficiadas/{Id}/imagem")
        {
            Uploads++;
            return new(FailImage ? HttpStatusCode.InternalServerError : HttpStatusCode.OK);
        }
        if (request.Method == HttpMethod.Delete && path == $"/api/empresas-beneficiadas/{Id}")
        {
            Deleted = true;
            return new(HttpStatusCode.NoContent);
        }
        if (request.Method == HttpMethod.Post && path == "/api/empresas-beneficiadas" || request.Method == HttpMethod.Put && path == $"/api/empresas-beneficiadas/{Id}")
        {
            Saved = await request.Content!.ReadFromJsonAsync<PVHSAUDE.Application.ViewModels.EmpresaBeneficiadaEntradaVm>(ct);
            Edited = request.Method == HttpMethod.Put;
            return Conflict ? new(HttpStatusCode.Conflict) : Json(Empresa, HttpStatusCode.Created);
        }
        throw new Exception($"Requisição inesperada (possível mistura com clínicas): {request.Method} {path}");
    }
    private static HttpResponseMessage Json(object value, HttpStatusCode status = HttpStatusCode.OK) => new(status) { Content = JsonContent.Create(value) };
}
