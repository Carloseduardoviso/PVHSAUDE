using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;

namespace PVHSAUDE.Api.Controllers;
[ApiController, Route("api/descontos")]
public class DescontosController(IDescontoService service) : ServiceController
{
    [HttpGet] public Task<IActionResult> Listar(CancellationToken ct) => Executar(async () => Ok(await service.ListarAsync(ct)));
    [HttpGet("{id:guid}")] public Task<IActionResult> Obter(Guid id, CancellationToken ct) => Executar(async () => Ok(await service.ObterAsync(id, ct)));
    [Authorize, HttpPost] public Task<IActionResult> Criar(PlanoEntradaVm vm, CancellationToken ct) => Executar(async () => { var d = await service.CriarAsync(vm, ct); return CreatedAtAction(nameof(Obter), new { id = d.Id }, d); });
    [Authorize, HttpPut("{id:guid}")] public Task<IActionResult> Atualizar(Guid id, PlanoEntradaVm vm, CancellationToken ct) => Executar(async () => { await service.AtualizarAsync(id, vm, ct); return NoContent(); });
}
