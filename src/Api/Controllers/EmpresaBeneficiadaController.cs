using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Controllers;

[Authorize, ApiController, Route("api/empresas-beneficiadas")]
public class EmpresaBeneficiadaController(IEmpresaBeneficiadaService service) : ServiceController
{
    [HttpGet] public Task<IActionResult> Listar(CancellationToken ct) => Executar(async () => Ok(await service.ListarAsync(ct)));
    [HttpGet("{id:guid}")] public Task<IActionResult> Obter(Guid id, CancellationToken ct) => Executar(async () => Ok(await service.ObterAsync(id, ct)));
    [Authorize, HttpPost] public Task<IActionResult> Criar(EmpresaBeneficiadaEntradaVm vm, CancellationToken ct) => Executar(async () =>
    { var p = await service.CriarAsync(vm, ct); return CreatedAtAction(nameof(Obter), new { id = p.Id }, p); });
    [Authorize, HttpPut("{id:guid}")] public Task<IActionResult> Atualizar(Guid id, EmpresaBeneficiadaEntradaVm vm, CancellationToken ct) => Executar(async () =>
    { await service.AtualizarAsync(id, vm, ct); return NoContent(); });
    // O arquivo continua limitado a 5 MB no serviço; a requisição inclui os cabeçalhos multipart.
    [Authorize, HttpPost("{id:guid}/imagem"), RequestSizeLimit(6_291_456)]
    public Task<IActionResult> UploadImagem(Guid id, IFormFile imagem, CancellationToken ct) => Executar(async () =>
    {
        await using var stream = imagem.OpenReadStream();
        return Ok(new { ImagemUrl = await service.UploadImagemAsync(id, imagem.FileName, imagem.Length, stream, ct) });
    });
}
