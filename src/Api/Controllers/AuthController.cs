using Infra.Data.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("Auth")]
public class AuthController(Context db, IPasswordHasher<Usuario> hasher, IAppJwtService jwt) : ControllerBase
{
    [HttpGet("sessao"), Authorize]
    public IActionResult Sessao() => Ok(User.FindAll(AcessoMenu.Claim).Select(x => x.Value).ToArray());
    [HttpPost("login"), AllowAnonymous, EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginVm model, CancellationToken ct)
    {
        var email = model.Email.Trim().ToUpperInvariant();
        var usuario = await db.Set<Usuario>().SingleOrDefaultAsync(x => x.EmailNormalizado == email, ct);
        // Also run the password derivation for unknown accounts.
        var result = hasher.VerifyHashedPassword(usuario ?? new Usuario(),
            usuario?.SenhaHash ?? DummyHash, model.Senha);
        if (usuario is null || !usuario.Ativo || result == PasswordVerificationResult.Failed)
            return Unauthorized();
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            usuario.SenhaHash = hasher.HashPassword(usuario, model.Senha);
            await db.SaveChangesAsync(ct);
        }
        var vm = new UsuarioVm { UsuarioId = usuario.Id, NomeCompleto = usuario.NomeCompleto, Email = usuario.Email, Role = usuario.Role, Menus = PVHSAUDE.Domain.Enuns.MenusAdministrativos.Ler(usuario.MenusPermitidos) };
        return Ok(new LoginResponse(jwt.GenereteToken(vm), vm));
    }
    private static readonly string DummyHash = new PasswordHasher<Usuario>().HashPassword(new Usuario(), Guid.NewGuid().ToString());
}
