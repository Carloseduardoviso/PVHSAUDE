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
}