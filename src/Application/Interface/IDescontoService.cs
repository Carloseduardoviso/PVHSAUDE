using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IDescontoService
{
    Task<List<DescontoRespostaVm>> ListarAsync(CancellationToken ct);
    Task<DescontoRespostaVm> ObterAsync(Guid id, CancellationToken ct);
    Task<DescontoRespostaVm> CriarAsync(CatalogoEntradaVm vm, CancellationToken ct);
    Task<DescontoRespostaVm> AtualizarAsync(Guid id, CatalogoEntradaVm vm, CancellationToken ct);
    Task ExcluirAsync(Guid id, CancellationToken ct);
}
