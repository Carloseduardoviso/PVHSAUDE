using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class BannerService(IBannerRepository repository, IUnitOfWork work, IMapper mapper) : IBannerService
{
    public async Task<List<BannerVm>> ListarAsync(bool ativos, CancellationToken ct) =>
        mapper.Map<List<BannerVm>>(await repository.ListarMetadadosAsync(ativos, ct));
    public async Task<BannerVm> ObterAsync(Guid id, CancellationToken ct) =>
        mapper.Map<BannerVm>(await repository.ObterMetadadosAsync(id, ct) ?? throw new ServiceException(ServiceError.NotFound));
    public async Task<ImagemVm> ImagemAsync(Guid id, bool autorizado, CancellationToken ct)
    {
        var entity = await repository.ObterAsync(x => x.Id == id && (x.Ativo || autorizado), ct) ?? throw new ServiceException(ServiceError.NotFound);
        return new ImagemVm(entity.Imagem, entity.ContentType);
    }
    public async Task<BannerVm> SalvarAsync(Guid? id, BannerVm vm, byte[]? imagem, CancellationToken ct)
    {
        var entity = id.HasValue ? await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound) : new Banner();
        if (!id.HasValue && imagem is null) throw new ServiceException(ServiceError.Invalid, "Selecione uma imagem.");
        if (imagem is not null)
        {
            if (imagem.Length == 0 || imagem.Length > 5_242_880)
                throw new ServiceException(ServiceError.Invalid, "A imagem deve ter até 5 MB.");
            var type = DetectarImagem(imagem);
            if (type is null) throw new ServiceException(ServiceError.Invalid, "Envie uma imagem JPG, PNG ou WEBP válida.");
            if (!BannerFormato.Valido(imagem, vm.Posicao)) throw new ServiceException(ServiceError.Invalid, BannerFormato.Mensagem(vm.Posicao));
            entity.Imagem = imagem;
            entity.ContentType = type;
        }
        mapper.Map(vm, entity);
        if (!id.HasValue) repository.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<BannerVm>(entity);
    }
    private static string? DetectarImagem(byte[] b)
    {
        if (b.Length >= 8 && b.AsSpan(0, 8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10})) return "image/png";
        if (b.Length >= 3 && b[0] == 255 && b[1] == 216 && b[2] == 255) return "image/jpeg";
        if (b.Length >= 12 && System.Text.Encoding.ASCII.GetString(b,0,4) == "RIFF" && System.Text.Encoding.ASCII.GetString(b,8,4) == "WEBP") return "image/webp";
        return null;
    }
}
