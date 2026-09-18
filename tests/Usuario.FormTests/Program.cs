using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Web.Controllers;
using Web.Services;

if (args.Contains("--whatsapp")) { await WhatsAppTests.Run(); return; }
if (args.Contains("--database")) { await UsuarioDatabaseTests.Run(); return; }

await BannerApiClientTests.Run();
if (args.Contains("--banner")) return;
AutoMapperTests.Run();
await CatalogoTests.Run();
await ContatoAvisosTests.Run();
await IntencaoAvisosTests.Run();

var clock = new TestTimeProvider(DateTimeOffset.UtcNow);
var builder = WebApplication.CreateBuilder(new WebApplicationOptions { ApplicationName = typeof(ContaController).Assembly.FullName });
builder.Logging.ClearProviders();
builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
builder.Services.AddControllersWithViews(o => o.Filters.Add<MenuAdministrativoFilter>()).AddApplicationPart(typeof(ContaController).Assembly);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.LoginPath = "/Conta/Login";
    o.AccessDeniedPath = "/Conta/AcessoNegado";
    o.Cookie.SecurePolicy = CookieSecurePolicy.None;
    o.ExpireTimeSpan = TimeSpan.FromHours(6);
    o.SlidingExpiration = false;
    o.TimeProvider = clock;
    o.EventsType = typeof(UsuarioCookieEvents);
});
builder.Services.AddScoped<UsuarioCookieEvents>();
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiAuthenticationHandler>();
var transport = new Transport();
builder.Services.AddSingleton(transport);
builder.Services.AddHttpClient("default", c => c.BaseAddress = new Uri("http://api/")).ConfigurePrimaryHttpMessageHandler(() => transport);
builder.Services.AddScoped(sp => new UsuarioApiClient(new HttpClient(transport) { BaseAddress = new Uri("http://api/") }, sp.GetRequiredService<IHttpContextAccessor>()));
builder.Services.AddHttpClient<PlanoApiClient>(c => c.BaseAddress = new Uri("http://api/"))
    .ConfigurePrimaryHttpMessageHandler(() => transport)
    .AddHttpMessageHandler<ApiAuthenticationHandler>();
builder.Services.AddHttpClient<ContatoApiClient>(c => c.BaseAddress = new Uri("http://api/"))
    .ConfigurePrimaryHttpMessageHandler(() => transport)
    .AddHttpMessageHandler<ApiAuthenticationHandler>();
builder.Services.AddHttpClient<IntencaoVendaApiClient>(c => c.BaseAddress = new Uri("http://api/"))
    .ConfigurePrimaryHttpMessageHandler(() => transport)
    .AddHttpMessageHandler<ApiAuthenticationHandler>();
await using var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapAreaControllerRoute("admin", "Administracao", "Administracao/{controller=Dashboard}/{action=Index}/{id?}").RequireAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Urls.Add("http://127.0.0.1:0");
await app.StartAsync();
var cookies = new CookieContainer();
using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, CookieContainer = cookies }) { BaseAddress = new Uri(app.Urls.Single()) };
void Check(bool condition, string message) { if (!condition) throw new Exception(message); Console.WriteLine("PASS: " + message); }
async Task<string> Token(string path)
{
    var html = await client.GetStringAsync(path);
    return WebUtility.HtmlDecode(Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
}
async Task<HttpResponseMessage> Login(Role role, string password = "senha-de-teste", string returnUrl = "https://outside.example")
{
    transport.Role = role;
    var url = "/Conta/Login?returnUrl=" + Uri.EscapeDataString(returnUrl);
    return await client.PostAsync(url, new FormUrlEncodedContent(new Dictionary<string,string> {
        ["__RequestVerificationToken"] = await Token(url), ["Email"] = "teste@example.com", ["Senha"] = password
    }));
}
using (var response = await client.GetAsync("/Administracao/Usuario"))
    Check(response.StatusCode == HttpStatusCode.Redirect && response.Headers.Location!.OriginalString.Contains("/Conta/Login"), "Anônimo precisa entrar.");
using (var response = await Login(Role.Comum, "incorreta"))
    Check(response.StatusCode == HttpStatusCode.OK && WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync()).Contains("inválidos"), "Senha inválida não autentica.");
using (var login = await Login(Role.Comum))
    Check(login.StatusCode == HttpStatusCode.Redirect, "Login cria uma sessão autenticada.");
using (var response = await client.GetAsync("/Administracao"))
    Check(response.StatusCode == HttpStatusCode.OK, "Sessão recém-criada acessa área administrativa.");
clock.Advance(TimeSpan.FromHours(4));
transport.Menus = ["Contato"];
using (var response = await client.GetAsync("/Administracao"))
    Check(response.StatusCode == HttpStatusCode.OK, "Atualização de permissões não encerra a sessão ativa.");
clock.Advance(TimeSpan.FromHours(2).Add(TimeSpan.FromTicks(1)));
using (var response = await client.GetAsync("/Administracao"))
{
    var location = response.Headers.Location ?? throw new Exception("Sessão expirada não informou o destino de login.");
    Check(response.StatusCode == HttpStatusCode.Redirect && location.AbsolutePath == "/Conta/Login" &&
        location.Query.Contains("ReturnUrl=%2FAdministracao", StringComparison.OrdinalIgnoreCase),
        "Sessão expirada redireciona ao login preservando o destino.");
    var returnUrl = Uri.UnescapeDataString(location.Query["?ReturnUrl=".Length..]);
    using var reauth = await Login(Role.Comum, returnUrl: returnUrl);
    Check(reauth.StatusCode == HttpStatusCode.Redirect && reauth.Headers.Location!.OriginalString == "/Administracao",
        "Novo login retorna ao destino solicitado antes da expiração.");
}
using (var response = await client.PostAsync("/Conta/Login", new FormUrlEncodedContent(new Dictionary<string,string> {
    ["__RequestVerificationToken"] = await Token("/Conta/Login"), ["Email"] = "teste@example.com"
})))
{
    var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
    Check(response.StatusCode == HttpStatusCode.OK && html.Contains("Informe sua senha."),
        "Senha obrigatória é exibida em português.");
}
foreach (var role in new[] { Role.Comum, Role.Gestor, Role.Administrador })
{
    using var login = await Login(role);
    Check(login.StatusCode == HttpStatusCode.Redirect && login.Headers.Location!.OriginalString == "/Administracao", "Login " + role + " rejeita redirecionamento externo.");
    using var response = await client.GetAsync("/Administracao/Usuario");
    Check(role == Role.Administrador ? response.StatusCode == HttpStatusCode.OK : response.StatusCode == HttpStatusCode.Redirect,
        "Cadastro restrito a Administrador: " + role);
}
using (var response = await client.GetAsync("/Administracao/Usuario"))
{
    var html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync());
    Check(html.Contains("alert alert-info") && html.Contains("Nenhum usuário cadastrado."),
        "Lista vazia de usuários usa alerta informativo.");
}
foreach (var (role, senha, duplicate, valid) in new[] { (0, "senha-de-teste", false, true), (1, "senha-de-teste", false, true), (2, "senha-de-teste", false, true), (99, "senha-de-teste", false, false), (0, "curta", false, false), (0, "senha-de-teste", true, false) })
{
    transport.Saved = null;
    transport.Duplicate = duplicate;
    using var response = await client.PostAsync("/Administracao/Usuario/Create", new FormUrlEncodedContent(new Dictionary<string,string> {
        ["__RequestVerificationToken"] = await Token("/Administracao/Usuario/Create"), ["NomeCompleto"] = "Usuário de teste",
        ["Email"] = "novo@example.com", ["Senha"] = senha, ["Role"] = role.ToString(), ["Menus"] = "Banner"
    }));
    Check(valid ? response.StatusCode == HttpStatusCode.Redirect && (int)transport.Saved!.Role == role : response.StatusCode == HttpStatusCode.OK,
        $"Formulário: role={role}, senha curta={senha.Length < 8}, duplicado={duplicate}");
    if (!valid && !duplicate) Check(transport.Saved is null, "Dados inválidos não chegam à API.");
    if (valid) Check(transport.Saved!.Menus.SequenceEqual(new[] { "Banner" }), "Cadastro encaminha menus selecionados.");
    if (!valid)
    {
        var html = await response.Content.ReadAsStringAsync();
        Check(!html.Contains("value=\"" + senha + "\""), "Senha não reaparece no formulário.");
        if (duplicate) Check(WebUtility.HtmlDecode(html).Contains("Já existe um usuário"), "E-mail duplicado mostra mensagem.");
    }
}
using (var response = await client.PostAsync("/Administracao/Usuario/Create", new FormUrlEncodedContent(new Dictionary<string,string>())))
    Check(response.StatusCode == HttpStatusCode.BadRequest, "Cadastro exige antiforgery.");
transport.Duplicate = false;
var editId = Guid.NewGuid();
foreach (var senha in new[] { "", "nova-senha-teste", "curta" })
{
    transport.Edited = null;
    using var response = await client.PostAsync($"/Administracao/Usuario/Edit/{editId}", new FormUrlEncodedContent(new Dictionary<string,string> {
        ["__RequestVerificationToken"] = await Token($"/Administracao/Usuario/Edit/{editId}"),
        ["UsuarioId"] = editId.ToString(), ["NomeCompleto"] = "Nome alterado", ["Email"] = "alterado@example.com", ["Role"] = "1", ["Senha"] = senha
    }));
    Check(senha == "curta" ? response.StatusCode == HttpStatusCode.OK && transport.Edited is null :
        response.StatusCode == HttpStatusCode.Redirect && transport.Edited?.Email == "alterado@example.com",
        "Edição valida senha opcional: " + senha.Length);
}
foreach (var action in new[] { "Inativar", "Ativar", "Excluir" })
{
    using var blocked = await client.PostAsync($"/Administracao/Usuario/{action}/{editId}", new FormUrlEncodedContent(new Dictionary<string,string>()));
    Check(blocked.StatusCode == HttpStatusCode.BadRequest, action + " exige antiforgery.");
    using var response = await client.PostAsync($"/Administracao/Usuario/{action}/{editId}", new FormUrlEncodedContent(new Dictionary<string,string> {
        ["__RequestVerificationToken"] = await Token("/Administracao/Usuario")
    }));
    Check(response.StatusCode == HttpStatusCode.Redirect && transport.Action == action.ToLowerInvariant(), action + " encaminhado à API.");
}
var usuario = new Usuario();
var hasher = new PasswordHasher<Usuario>();
var hash = hasher.HashPassword(usuario, "senha-de-teste");
Check(hash != "senha-de-teste" && hasher.VerifyHashedPassword(usuario, hash, "senha-de-teste") != PasswordVerificationResult.Failed
    && hasher.VerifyHashedPassword(usuario, hash, "outra") == PasswordVerificationResult.Failed, "Hash verifica senha correta e rejeita incorreta.");
using (var response = await client.PostAsync("/Conta/Sair", new FormUrlEncodedContent(new Dictionary<string,string> {
    ["__RequestVerificationToken"] = await Token("/Administracao/Usuario")
}))) Check(response.StatusCode == HttpStatusCode.Redirect, "Logout concluído.");
using (var response = await client.GetAsync("/Administracao/Usuario"))
    Check(response.StatusCode == HttpStatusCode.Redirect, "Logout encerra acesso.");
transport.Menus = ["Banner"];
using (var login = await Login(Role.Comum)) { }
var inicio = await client.GetStringAsync("/Administracao");
Check(inicio.Contains("href=\"/Administracao/Banner\"") && !inicio.Contains("href=\"/Administracao/Contato\"") && !inicio.Contains("href=\"/Administracao/Usuario\""), "Menu mostra apenas as permissões do usuário.");
using (var forbidden = await client.GetAsync("/Administracao/Contato"))
    Check(forbidden.StatusCode == HttpStatusCode.Redirect && forbidden.Headers.Location!.OriginalString.Contains("AcessoNegado"), "URL direta de menu não permitido é bloqueada.");
transport.Menus = ["Contato"];
inicio = await client.GetStringAsync("/Administracao");
Check(!inicio.Contains("href=\"/Administracao/Banner\"") && inicio.Contains("href=\"/Administracao/Contato\""), "Sessão aberta recebe alteração dos menus.");
using (var forbidden = await client.GetAsync("/Administracao/Banner"))
    Check(forbidden.StatusCode == HttpStatusCode.Redirect && forbidden.Headers.Location!.OriginalString.Contains("AcessoNegado"), "Revogação bloqueia URL sem novo login.");
transport.Menus = [];
inicio = await client.GetStringAsync("/Administracao");
Check(!inicio.Contains("href=\"/Administracao/Contato\""), "Nenhum menu selecionado mantém somente Home.");
foreach (var role in new[] { Role.Comum, Role.Gestor, Role.Administrador })
{
    transport.Menus = [];
    using var loginSemMenus = await Login(role);
    var paginaSemMenus = await client.GetStringAsync("/Administracao");
    foreach (var menu in MenusAdministrativos.Opcoes.Keys)
    {
        var destino = menu is "Especialidades" or "Procedimentos" ? "Catalogo/" + menu : menu;
        Check(!paginaSemMenus.Contains("href=\"/Administracao/" + destino + "\""), "Menu não selecionado oculto: " + role + "/" + menu);
    }
    using var acessoSemMenu = await client.GetAsync("/Administracao/Contato");
    Check(acessoSemMenu.StatusCode == HttpStatusCode.Redirect && acessoSemMenu.Headers.Location!.OriginalString.Contains("AcessoNegado"), "Acesso direto sem permissão bloqueado: " + role);
    transport.Menus = ["Contato"];
    var paginaComMenu = await client.GetStringAsync("/Administracao");
    Check(paginaComMenu.Contains("href=\"/Administracao/Contato\"") && !paginaComMenu.Contains("href=\"/Administracao/Banner\""), "Seleção respeitada após atualização: " + role);
}
await MenuPermissionsTests.Run();
await app.StopAsync();

sealed class Transport : HttpMessageHandler
{
    public string[] Menus = [];
    public Role Role;
    public bool Duplicate;
    public UsuarioCadastroVm? Saved;
    public UsuarioEdicaoVm? Edited;
    public string? Action;
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (request.RequestUri!.AbsolutePath == "/Auth/login")
        {
            var login = await request.Content!.ReadFromJsonAsync<LoginVm>(ct);
            if (login!.Senha != "senha-de-teste") return new(HttpStatusCode.Unauthorized);
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new LoginResponse("test-token", new UsuarioVm {
                UsuarioId = Guid.NewGuid(), NomeCompleto = "Teste", Email = login.Email, Role = Role, Menus = Menus
            })) };
        }
        if (request.Headers.Authorization?.Parameter != "test-token") throw new Exception("Token não encaminhado.");
        if (request.RequestUri!.AbsolutePath == "/Auth/sessao")
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(Menus) };
        if (request.RequestUri!.AbsolutePath.StartsWith("/api/usuarios/"))
        {
            if (request.Method == HttpMethod.Get)
                return new(HttpStatusCode.OK) { Content = JsonContent.Create(new UsuarioVm { UsuarioId = Guid.Parse(request.RequestUri.Segments.Last()), NomeCompleto = "Teste", Email = "teste@example.com" }) };
            if (request.Method == HttpMethod.Put) Edited = await request.Content!.ReadFromJsonAsync<UsuarioEdicaoVm>(ct);
            Action = request.Method == HttpMethod.Delete ? "excluir" : request.RequestUri.Segments.Last();
            return new(HttpStatusCode.NoContent);
        }
        if (request.Method == HttpMethod.Get)
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<UsuarioVm>()) };
        Saved = await request.Content!.ReadFromJsonAsync<UsuarioCadastroVm>(ct);
        return new(Duplicate ? HttpStatusCode.Conflict : HttpStatusCode.Created);
    }
}

sealed class TestTimeProvider(DateTimeOffset utcNow) : TimeProvider
{
    private DateTimeOffset _utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void Advance(TimeSpan duration) => _utcNow = _utcNow.Add(duration);
}
