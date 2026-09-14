using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using Microsoft.AspNetCore.RateLimiting;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("Auth")]
public class AuthController(IAuthService service) : ServiceController
{
    [HttpGet("sessao"), Authorize]
    public IActionResult Sessao() => Ok(User.FindAll(AcessoMenu.Claim).Select(x => x.Value).ToArray());
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("login")]
    public Task<IActionResult> Login(LoginVm vm, CancellationToken ct) => Executar(async () => Ok(await service.LoginAsync(vm, ct)));
}