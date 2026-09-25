using Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("172.28.0.2"));
});

// Add services to the container.
builder.Services.AddControllersWithViews(options => options.Filters.Add<MenuAdministrativoFilter>());
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Conta/Login";
    options.AccessDeniedPath = "/Conta/AcessoNegado";
    options.Cookie.Name = "PVHSAUDE.Administracao";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(6);
    options.SlidingExpiration = false;
    options.EventsType = typeof(Web.Services.UsuarioCookieEvents);
}).AddCookie(BeneficiarioAuthentication.Scheme, options =>
{
    options.LoginPath = "/Beneficiario/BeneficiarioConta/Login";
    options.AccessDeniedPath = "/Beneficiario/BeneficiarioConta/Login";
    options.Cookie.Name = "PVHSAUDE.Beneficiario";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromHours(6);
    options.SlidingExpiration = false;
    options.EventsType = typeof(UsuarioCookieEvents);
});
builder.Services.AddScoped<Web.Services.UsuarioCookieEvents>();
builder.Services.AddScoped<WhatsAppApiClient>();
builder.Services.AddScoped<LogoPortalStorage>();
builder.Services.AddHttpClient<CepConsultaClient>(client => client.Timeout = TimeSpan.FromSeconds(45));
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<BannerApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));
builder.Services.AddHttpClient<ContatoApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));
builder.Services.AddHttpClient<UsuarioApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));
// API clients use the current HTTP context to forward authentication.
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiAuthenticationHandler>();
builder.Services.AddTransient<ApiConnectionRetryHandler>();
builder.Services.ConfigureHttpClientDefaults(http => http
    .AddHttpMessageHandler<ApiAuthenticationHandler>()
    .AddHttpMessageHandler<ApiConnectionRetryHandler>());
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("default", client => client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));

builder.Services.AddHttpClient<BeneficiarioApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));

builder.Services.AddHttpClient<PlanoApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));
builder.Services.AddHttpClient<DescontoApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));

builder.Services.AddHttpClient<CredenciadoApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));

builder.Services.AddHttpClient<EmpresaBeneficiadaApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));
builder.Services.AddHttpClient<IntencaoVendaApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));
builder.Services.AddHttpClient<AcessoBeneficiarioApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:44319/"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapAreaControllerRoute(
    name: "beneficiario",
    areaName: "Beneficiario",
    pattern: "Beneficiario/{controller=MinhaArea}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "administracao",
    areaName: "Administracao",
    pattern: "Administracao/{controller=Dashboard}/{action=Index}/{id?}")
    .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Administrador,Gestor,Comum" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapGet("/MinhaArea/{action?}", (string? action) =>
    Results.Redirect("/Beneficiario/MinhaArea/" + (string.IsNullOrWhiteSpace(action) ? "Index" : action)));
app.MapGet("/BeneficiarioConta/{action?}", (string? action) =>
    Results.Redirect("/Beneficiario/BeneficiarioConta/" + (string.IsNullOrWhiteSpace(action) ? "Login" : action)));


app.MapGet("/health", () => Results.Ok()).AllowAnonymous();

app.Run();
