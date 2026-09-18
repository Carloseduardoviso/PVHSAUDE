using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/especialidades")]
public class EspecialidadesController(ICatalogoService service) : ServiceController
{
    [HttpGet] public Task<IActionResult> Get(CancellationToken ct) => Executar(async () => Ok(await service.EspecialidadesAsync(ct)));
    [Authorize, HttpPost] public Task<IActionResult> Post(CatalogoEntradaVm vm, CancellationToken ct) => Executar(async () => Ok(await service.CriarEspecialidadeAsync(vm, ct)));
    [Authorize, HttpPut("{id:guid}")] public Task<IActionResult> Put(Guid id, CatalogoEntradaVm vm, CancellationToken ct) => Executar(async () => Ok(await service.AtualizarEspecialidadeAsync(id, vm, ct)));
    [Authorize, HttpDelete("{id:guid}")] public Task<IActionResult> Delete(Guid id, CancellationToken ct) => Executar(async () => { await service.ExcluirEspecialidadeAsync(id, ct); return NoContent(); });
}
