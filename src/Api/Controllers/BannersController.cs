using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using System.ComponentModel.DataAnnotations;
namespace PVHSAUDE.Api.Controllers;

[ApiController, Route("api/banners")]
public class BannersController(IBannerService service) : ServiceController
{
    [HttpGet, Authorize] public Task<IActionResult> Listar(CancellationToken ct) => Executar(async () => Ok(await service.ListarAsync(false, ct)));
    [HttpGet("ativos"), AllowAnonymous] public Task<IActionResult> Ativos(CancellationToken ct) => Executar(async () => Ok(await service.ListarAsync(true, ct)));
    [HttpGet("{id:guid}"), Authorize] public Task<IActionResult> Obter(Guid id, CancellationToken ct) => Executar(async () => Ok(await service.ObterAsync(id, ct)));
    [HttpGet("{id:guid}/imagem")] public Task<IActionResult> Imagem(Guid id, CancellationToken ct) => Executar(async () =>
    {
        var imagem = await service.ImagemAsync(id, AcessoMenu.PodeAcessar(User, "Banner"), ct);
        Response.Headers.CacheControl = "no-store";
        Response.Headers.XContentTypeOptions = "nosniff";
        return File(imagem.Conteudo, imagem.ContentType);
    });
    [HttpPost, Authorize, RequestSizeLimit(6_291_456)]
    public Task<IActionResult> Criar([FromForm] BannerRequest request, CancellationToken ct) => Salvar(null, request, ct);
    [HttpPut("{id:guid}"), Authorize, RequestSizeLimit(6_291_456)]
    public Task<IActionResult> Editar(Guid id, [FromForm] BannerRequest request, CancellationToken ct) => Salvar(id, request, ct);
    private Task<IActionResult> Salvar(Guid? id, BannerRequest request, CancellationToken ct) => Executar(async () =>
    {
        byte[]? imagem = null;
        if (request.Imagem is not null)
        {
            await using var stream = new MemoryStream();
            await request.Imagem.CopyToAsync(stream, ct);
            imagem = stream.ToArray();
        }
        return Ok(await service.SalvarAsync(id, new BannerVm { Titulo = request.Titulo, Posicao = request.Posicao, Ativo = request.Ativo }, imagem, ct));
    });
}

public class BannerRequest
{
    [Required, StringLength(200)]
    public string Titulo { get; set; } = "";
    public PVHSAUDE.Domain.Enuns.PosicaoBanner Posicao { get; set; } = PVHSAUDE.Domain.Enuns.PosicaoBanner.Central;
    public bool Ativo { get; set; } = true;
    public IFormFile? Imagem { get; set; }
}