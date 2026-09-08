using AutoMapper.Extensions.ExpressionMapping;
using Infra.Data.Base;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Application.AppService;
using PVHSAUDE.Application.AutoMapper;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Infra.Helper.Settings;
using PVHSAUDE.Api.Configs;
using PVHSAUDE.Api.Middleware;
using PVHSAUDE.Infra.Ioc;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Context>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddOptions<JwtSetting>().BindConfiguration(JwtSetting.Config).ValidateDataAnnotations().ValidateOnStart();
builder.Services.AddScoped<IAppJwtService, AppJwtService>();

builder.Services.AddAutoMapper(config =>
{
    config.AddExpressionMapping();
}, typeof(AutoMapperConfig));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthenticationConfig();
builder.Services.AddInfrastructure();

builder.Services.AddConfigRatesLimiter();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMetricsAuthMiddleware();

app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

app.UseValidateTokenForgotPasswordMiddleware();

app.MapControllers();

app.Run();
