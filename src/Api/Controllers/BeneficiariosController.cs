using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/beneficiarios"), Authorize]
public class BeneficiariosController(IBeneficiarioService service) : ServiceController
{
    [HttpGet] public Task<IActionResult> Listar(CancellationToken ct) => Executar(async () => Ok(await service.ListarAsync(ct)));
    [HttpGet("{id:guid}")] public Task<IActionResult> Obter(Guid id, CancellationToken ct) => Executar(async () => Ok(await service.ObterAsync(id, ct)));
    [HttpPost] public Task<IActionResult> Criar(BeneficiarioEntradaVm vm, CancellationToken ct) => Executar(async () =>
    { var p = await service.CriarAsync(vm, ct); return CreatedAtAction(nameof(Obter), new { id = p.Id }, p); });
    [HttpPut("{id:guid}")] public Task<IActionResult> Atualizar(Guid id, BeneficiarioEntradaVm vm, CancellationToken ct) => Executar(async () =>
    { await service.AtualizarAsync(id, vm, ct); return NoContent(); });
    [HttpPost("{id:guid}/inativar")] public Task<IActionResult> Inativar(Guid id, CancellationToken ct) => Executar(async () =>
    { await service.InativarAsync(id, ct); return NoContent(); });
    [HttpDelete("{id:guid}")] public Task<IActionResult> Excluir(Guid id, CancellationToken ct) => Executar(async () =>
    { await service.ExcluirAsync(id, ct); return NoContent(); });
}