using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/usuarios"), Authorize(Roles = "Administrador")]
public class UsuariosController(IUsuarioService service) : ServiceController
{
    [HttpGet] public Task<IActionResult> Listar(CancellationToken ct) => Executar(async () => Ok(await service.ListarAsync(ct)));
    [HttpGet("{id:guid}")] public Task<IActionResult> Obter(Guid id, CancellationToken ct) => Executar(async () => Ok(await service.ObterAsync(id, ct)));
    [HttpPost] public Task<IActionResult> Criar(UsuarioCadastroVm vm, CancellationToken ct) => Executar(async () => StatusCode(201, await service.CriarAsync(vm, ct)));
    [HttpPut("{id:guid}")] public Task<IActionResult> Editar(Guid id, UsuarioEdicaoVm vm, CancellationToken ct) => Executar(async () =>
    { await service.EditarAsync(id, vm, ct); return NoContent(); });
    [HttpPost("{id:guid}/inativar")] public Task<IActionResult> Inativar(Guid id, CancellationToken ct) => Executar(async () =>
    { await service.InativarAsync(id, ct); return NoContent(); });
    [HttpPost("{id:guid}/ativar")] public Task<IActionResult> Ativar(Guid id, CancellationToken ct) => Executar(async () =>
    { await service.AtivarAsync(id, ct); return NoContent(); });
    [HttpDelete("{id:guid}")] public Task<IActionResult> Excluir(Guid id, CancellationToken ct) => Executar(async () =>
    { await service.ExcluirAsync(id, ct); return NoContent(); });
}