using Microsoft.AspNetCore.Mvc;
using Web.Services;

namespace PVHSAUDE.Web.Areas.Administracao.Controllers;

[Area("Administracao")]
public class CepController(CepConsultaClient ceps) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Consultar(string cep, CancellationToken ct)
    {
        try
        {
            var endereco = await ceps.ConsultarAsync(cep, ct);
            return endereco is null ? NotFound() : Ok(endereco);
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }
}
