using PVHSAUDE.Infra.Ioc;
using Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
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
});
builder.Services.AddScoped<Web.Services.UsuarioCookieEvents>();
builder.Services.AddAuthorization();
builder.Services.AddHttpClient<BannerApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7283/"));
builder.Services.AddHttpClient<UsuarioApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7283/"));
// Infra.Auth services depend on these framework services.
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<ApiAuthenticationHandler>();
builder.Services.ConfigureHttpClientDefaults(http => http.AddHttpMessageHandler<ApiAuthenticationHandler>());
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("default", client => client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7283/"));
builder.Services.AddHttpClient<IBeneficiarioApiClient, BeneficiarioApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7283/"));
builder.Services.AddHttpClient<PlanoApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7283/"));
builder.Services.AddHttpClient<CredenciadoApiClient>(client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:BaseUrl"] ?? "https://localhost:7283/"));
builder.Services.AddInfrastructure();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapAreaControllerRoute(
    name: "administracao",
    areaName: "Administracao",
    pattern: "Administracao/{controller=Dashboard}/{action=Index}/{id?}")
    .RequireAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
