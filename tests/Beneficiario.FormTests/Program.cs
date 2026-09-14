using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.DataProtection;
using Web.Models;
using Web.Services;
using PVHSAUDE.Web.Areas.Administracao.Controllers;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName = typeof(BeneficiarioController).Assembly.FullName });
builder.Logging.ClearProviders();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddControllersWithViews().AddApplicationPart(typeof(BeneficiarioController).Assembly);
var transport = new ApiTransport();
var api = new HttpClient(transport) { BaseAddress = new Uri("http://test-api/") };
builder.Services.AddSingleton(new PlanoApiClient(api));
builder.Services.AddSingleton(new CredenciadoApiClient(api));
builder.Services.AddSingleton(new BeneficiarioApiClient(api));
await using var app = builder.Build();
app.MapAreaControllerRoute("admin", "Administracao", "Administracao/{controller=Dashboard}/{action=Index}/{id?}");
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = new Uri(app.Urls.Single()) };
foreach (var (count, invalidDate, semEmpresa) in new[] { (0, false, false), (1, false, false), (5, false, false), (6, false, false), (1, true, false), (0, false, true) })
{
    var html = await client.GetStringAsync("/Administracao/Beneficiario/Create");
    var token = WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    if (token.Length == 0) throw new Exception("Antiforgery token not rendered.");
    var fields = new List<KeyValuePair<string, string>>();
    void Add(string key, string value) => fields.Add(new(key, value));
    Add("__RequestVerificationToken", token);
    Add("Nome", "Cadastro de teste");
    Add("Cpf", "123.456.789-01");
    Add("DataNascimento", "1990-05-12");
    Add("PlanoId", Guid.NewGuid().ToString());
    Add("CredenciadoId", semEmpresa ? "" : ApiTransport.EmpresaId.ToString());
    Add("DataInicio", "2026-01-01");
    Add("DataValidade", "2027-01-01");
    Add("Status", "2");
    for (var i = 0; i < count; i++)
    {
        Add("Dependentes.Index", i.ToString());
        Add($"Dependentes[{i}].Id", Guid.Empty.ToString());
        Add($"Dependentes[{i}].Nome", $"Dependente teste {i}");
        Add($"Dependentes[{i}].Cpf", "123.456.789-01");
        Add($"Dependentes[{i}].DataNascimento", invalidDate ? "" : "2015-02-03");
        Add($"Dependentes[{i}].GrauParentesco", "3");
    }
    transport.Saved = null;
    using var response = await client.PostAsync("/Administracao/Beneficiario/Create", new FormUrlEncodedContent(fields));
    if (count <= 5 && !invalidDate && !semEmpresa)
    {
        if (transport.Saved?.CredenciadoId != ApiTransport.EmpresaId)
            throw new Exception("The company selection was not sent correctly.");
        if (transport.Saved?.PlanoId != ApiTransport.PlanoId)
            throw new Exception("The plan must be derived from the company, ignoring the submitted plan.");
        if (response.StatusCode != HttpStatusCode.Redirect || transport.Saved?.Dependentes.Count != count)
        {
            var body = await response.Content.ReadAsStringAsync();
            Console.WriteLine(Regex.Replace(body, "<[^>]+>", " "));
            throw new Exception($"Expected save with {count} dependents. HTTP {(int)response.StatusCode}, sent {transport.Saved?.Dependentes.Count}.");
        }
    }
    else if (transport.Saved != null || response.StatusCode != HttpStatusCode.OK)
        throw new Exception("Invalid submissions must be rejected.");
    if (invalidDate)
    {
        var body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
        if (!body.Contains("Informe a data de nascimento.") || !body.Contains("Dependente teste 0"))
            throw new Exception("Validation must display the error and preserve the dependent.");
    }
    Console.WriteLine($"PASS: {count} dependents, invalid date: {invalidDate}, missing company: {semEmpresa}, plan derived from company.");
}
foreach (var invalid in new[] { false, true })
{
    var html = await client.GetStringAsync("/Administracao/Credenciado/Create");
    var token = WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    transport.Empresa = null;
    using var response = await client.PostAsync("/Administracao/Credenciado/Create", new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["__RequestVerificationToken"] = token,
        ["RazaoSocial"] = "Empresa de teste",
        ["NomeFantasia"] = "Clínica de teste",
        ["Cnpj"] = invalid ? "123" : "12.345.678/0001-90",
        ["Telefone"] = "(69) 99999-8888",
        ["Tipo"] = "1",
        ["PlanoId"] = ApiTransport.PlanoId.ToString(),
        ["StatusCredenciamento"] = "1"
    }));
    if (!invalid && (response.StatusCode != HttpStatusCode.Redirect || transport.Empresa?.NomeFantasia != "Clínica de teste" || transport.Empresa?.PlanoId != ApiTransport.PlanoId))
        throw new Exception("Company form failed to send.");
    if (invalid && (response.StatusCode != HttpStatusCode.OK || transport.Empresa != null ||
        !WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()).Contains("Informe um CNPJ completo.")))
        throw new Exception("Company form must display validation errors.");
    Console.WriteLine($"PASS: company form, invalid CNPJ: {invalid}.");
}
await app.StopAsync();

sealed class ApiTransport : HttpMessageHandler
{
    public static readonly Guid PlanoId = Guid.NewGuid();
    public static readonly Guid EmpresaId = Guid.NewGuid();
    public BeneficiarioVm? Saved;
    public CredenciadoVm? Empresa;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/api/credenciados")
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new[] { new CredenciadoVm { Id = EmpresaId, NomeFantasia = "Empresa teste", PlanoId = PlanoId } }) };
        if (request.RequestUri!.AbsolutePath is "/api/especialidades" or "/api/procedimentos")
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<CatalogoItemVm>()) };
        if (request.RequestUri!.AbsolutePath == "/api/planos")
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new[] { new PlanoVm { Id = PlanoId, Nome = "Plano teste", Valor = 99.90m } }) };
        if (request.Method == HttpMethod.Post && request.RequestUri.AbsolutePath == "/api/beneficiarios")
        {
            Saved = await request.Content!.ReadFromJsonAsync<BeneficiarioVm>(ct);
            return new(HttpStatusCode.Created);
        }
        if (request.Method == HttpMethod.Post && request.RequestUri.AbsolutePath == "/api/credenciados")
        {
            Empresa = await request.Content!.ReadFromJsonAsync<CredenciadoVm>(ct);
            return new(HttpStatusCode.Created);
        }
        throw new Exception("Unexpected API request.");
    }
}
