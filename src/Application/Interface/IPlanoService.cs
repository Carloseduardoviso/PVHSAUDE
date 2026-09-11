using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IPlanoService
{
    Task<List<PlanoRespostaVm>> ListarAsync(CancellationToken ct);
    Task<PlanoRespostaVm> ObterAsync(Guid id, CancellationToken ct);
    Task<PlanoRespostaVm> CriarAsync(PlanoEntradaVm vm, CancellationToken ct);
    Task AtualizarAsync(Guid id, PlanoEntradaVm vm, CancellationToken ct);
}
