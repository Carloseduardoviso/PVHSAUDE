using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/portal"), AllowAnonymous]
public class PortalController(IPortalService service) : ServiceController
{
    [HttpGet("planos-empresas"), ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public Task<IActionResult> PlanosPorEmpresa(CancellationToken ct) => Executar(async () => Ok(await service.PlanosPorEmpresaAsync(ct)));
}