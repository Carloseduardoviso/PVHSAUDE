using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IBannerService
{
    Task<List<BannerVm>> ListarAsync(bool ativos, CancellationToken ct);
    Task<BannerVm> ObterAsync(Guid id, CancellationToken ct);
    Task<ImagemVm> ImagemAsync(Guid id, bool autorizado, CancellationToken ct);
    Task<BannerVm> SalvarAsync(Guid? id, BannerVm vm, byte[]? imagem, CancellationToken ct);
}
