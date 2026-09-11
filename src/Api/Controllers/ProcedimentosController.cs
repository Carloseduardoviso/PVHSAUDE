using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/procedimentos")]
public class ProcedimentosController(ICatalogoService service) : ServiceController
{
    [HttpGet] public Task<IActionResult> Get(CancellationToken ct) => Executar(async () => Ok(await service.ProcedimentosAsync(ct)));
    [Authorize, HttpPost] public Task<IActionResult> Post(CatalogoEntradaVm vm, CancellationToken ct) => Executar(async () => Ok(await service.CriarProcedimentoAsync(vm, ct)));
}