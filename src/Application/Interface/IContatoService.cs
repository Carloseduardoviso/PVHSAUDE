using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IContatoService
{
    Task<List<ContatoVm>> ListarAsync(CancellationToken ct);
    Task<ContatoVm> ObterAsync(Guid id, CancellationToken ct);
    Task CriarAsync(ContatoEntradaVm vm, CancellationToken ct);
    Task ExcluirAsync(Guid id, CancellationToken ct);
}
