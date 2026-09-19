using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/credenciados")]
public class CredenciadosController(ICredenciadoService service) : ServiceController
{
    private CredenciadoRespostaVm UrlPublica(CredenciadoRespostaVm vm) =>
        vm with
        {
            ImagemUrl = vm.ImagemUrl is null ? null : $"{Request.Scheme}://{Request.Host}{vm.ImagemUrl}",
            ImagemUrls = vm.ImagemUrls?.Select(url => $"{Request.Scheme}://{Request.Host}{url}").ToList()
        };
    [HttpGet] public Task<IActionResult> Listar(CancellationToken ct) => Executar(async () => Ok((await service.ListarAsync(ct)).Select(UrlPublica)));
    [HttpGet("{id:guid}")] public Task<IActionResult> Obter(Guid id, CancellationToken ct) => Executar(async () => Ok(UrlPublica(await service.ObterAsync(id, ct))));
    [Authorize, HttpPost] public Task<IActionResult> Criar(CredenciadoEntradaVm vm, CancellationToken ct) => Executar(async () =>
    { var p = await service.CriarAsync(vm, ct); return CreatedAtAction(nameof(Obter), new { id = p.Id }, UrlPublica(p)); });
    [Authorize, HttpPut("{id:guid}")] public Task<IActionResult> Atualizar(Guid id, CredenciadoEntradaVm vm, CancellationToken ct) => Executar(async () =>
    { await service.AtualizarAsync(id, vm, ct); return NoContent(); });
    [Authorize, HttpPost("{id:guid}/imagem"), RequestSizeLimit(5_242_880)]
    public Task<IActionResult> UploadImagem(Guid id, IFormFile imagem, CancellationToken ct) => Executar(async () =>
    {
        await using var stream = imagem.OpenReadStream();
        return Ok(new { ImagemUrl = await service.UploadImagemAsync(id, imagem.FileName, imagem.Length, stream, ct) });
    });
}
