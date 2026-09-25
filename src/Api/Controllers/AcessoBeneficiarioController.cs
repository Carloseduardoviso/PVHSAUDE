using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;

namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/acesso-beneficiario")]
public class AcessoBeneficiarioController(IPortalAcessoService service) : ServiceController
{
    [HttpPost("cadastro"), AllowAnonymous, EnableRateLimiting("login")]
    public Task<IActionResult> Cadastro(CadastroAcessoBeneficiarioVm model, CancellationToken ct) =>
        Executar(async () => { await service.RegistrarAsync(model, ct); return Created("/api/acesso-beneficiario/me", null); });

    [HttpPost("emitir"), Authorize(Roles = "Administrador")]
    public Task<IActionResult> Emitir(EmitirAcessoBeneficiarioVm model, CancellationToken ct) =>
        Executar(async () => Ok(await service.EmitirAcessoAsync(model.Cpf, ct)));

    [HttpGet("me"), Authorize(Roles = "Beneficiario")]
    public Task<IActionResult> Me(CancellationToken ct) => Executar(async () =>
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId)) return Unauthorized();
        return Ok(await service.MinhaAreaAsync(usuarioId, ct));
    });

    [HttpGet("pendentes"), Authorize(Roles = "Administrador")]
    public Task<IActionResult> Pendentes(CancellationToken ct) =>
        Executar(async () => Ok(await service.ListarPendentesAsync(ct)));

    [HttpPost("pendentes/{usuarioId:guid}/aprovar"), Authorize(Roles = "Administrador")]
    public Task<IActionResult> Aprovar(Guid usuarioId, AprovarAcessoBeneficiarioVm model, CancellationToken ct) =>
        Executar(async () => { await service.AprovarAsync(usuarioId, model.BeneficiarioId, ct); return NoContent(); });
}

public record AprovarAcessoBeneficiarioVm(Guid BeneficiarioId);
public record EmitirAcessoBeneficiarioVm(string Cpf);
