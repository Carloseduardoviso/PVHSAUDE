using AutoMapper.Extensions.ExpressionMapping;
using Infra.Data.Base;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Api.Configs;
using PVHSAUDE.Api.Middleware;
using PVHSAUDE.Api.Services;
using PVHSAUDE.Application.AppService;
using PVHSAUDE.Application.AutoMapper;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Infra.Helper.Settings;
using PVHSAUDE.Infra.Ioc;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using System.Threading.RateLimiting;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Parse("172.29.0.3"));
});

builder.Services.AddDbContext<Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOptions<JwtSetting>().BindConfiguration(JwtSetting.Config).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddScoped<IAppJwtService, AppJwtService>();

builder.Services.AddAutoMapper(config =>
{
    config.AddExpressionMapping();
}, typeof(AutoMapperConfig));

builder.Services.AddControllers(options => options.Filters.Add<MenuApiFilter>());
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context => RateLimitPartition.GetFixedWindowLimiter(
        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
        _ => new FixedWindowRateLimiterOptions { PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0 }));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthenticationConfig();
builder.Services.AddInfrastructure();
builder.Services.AddApplicationServices();
builder.Services.AddScoped<IImagemStorage, ImagemStorage>();
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection(EmailOptions.Section));
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddHostedService<EmailReminderWorker>();

builder.Services.AddConfigRatesLimiter();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

var app = builder.Build();
using (var migrationScope = app.Services.CreateScope())
{
    await migrationScope.ServiceProvider.GetRequiredService<Context>().Database.MigrateAsync();
}
if (args.Contains("--criar-administrador"))
{
    await AdministradorInicial.CriarAsync(app.Services);
    return;
}

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
if (!app.Environment.IsDevelopment()) app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMetricsAuthMiddleware();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.UseValidateTokenForgotPasswordMiddleware();

app.MapControllers();

app.MapGet("/health", () => Results.Ok()).AllowAnonymous();

app.Run();
