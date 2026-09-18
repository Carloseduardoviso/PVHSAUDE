using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/intencoes-venda")]
public class IntencaoVendaController(IIntencaoVendaService service) : ServiceController
{
    [HttpPost, AllowAnonymous]
    public Task<IActionResult> Criar(IntencaoVendaEntradaVm vm, CancellationToken ct) => Executar(async () => { await service.CriarAsync(vm, ct); return Ok(); });

    [HttpGet, Authorize]
    public Task<IActionResult> Listar(CancellationToken ct) => Executar(async () => Ok(await service.ListarAsync(ct)));

    [HttpPut("{id:guid}/status"), Authorize]
    public Task<IActionResult> AtualizarStatus(Guid id, [FromBody] StatusIntencaoVenda status, CancellationToken ct) => Executar(async () => { await service.AtualizarStatusAsync(id, status, ct); return NoContent(); });

    [HttpPatch("{id:guid}/suspender-notificacao"), Authorize]
    public Task<IActionResult> SuspenderNotificacao(Guid id, CancellationToken ct) => Executar(async () => { await service.SuspenderNotificacaoAsync(id, ct); return NoContent(); });
}
