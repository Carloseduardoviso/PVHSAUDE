using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/contatos")]
public class ContatosController(IContatoService service) : ServiceController
{
    [HttpGet, Authorize] public Task<IActionResult> Listar(CancellationToken ct) => Executar(async () => Ok(await service.ListarAsync(ct)));
    [HttpGet("{id:guid}"), Authorize] public Task<IActionResult> Obter(Guid id, CancellationToken ct) => Executar(async () => Ok(await service.ObterAsync(id, ct)));
    [HttpPatch("{id:guid}/suspender-notificacao"), Authorize] public Task<IActionResult> SuspenderNotificacao(Guid id, CancellationToken ct) => Executar(async () =>
    { await service.SuspenderNotificacaoAsync(id, ct); return NoContent(); });
    [HttpDelete("{id:guid}"), Authorize] public Task<IActionResult> Excluir(Guid id, CancellationToken ct) => Executar(async () =>
    { await service.ExcluirAsync(id, ct); return NoContent(); });
    [HttpPost, AllowAnonymous] public Task<IActionResult> Criar(ContatoEntradaVm vm, CancellationToken ct) => Executar(async () =>
    { await service.CriarAsync(vm, ct); return Ok(); });
}
