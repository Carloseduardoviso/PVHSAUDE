using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IBeneficiarioService
{
    Task<List<BeneficiarioRespostaVm>> ListarAsync(CancellationToken ct);
    Task<BeneficiarioRespostaVm> ObterAsync(Guid id, CancellationToken ct);
    Task<BeneficiarioRespostaVm> CriarAsync(BeneficiarioEntradaVm vm, CancellationToken ct);
    Task AtualizarAsync(Guid id, BeneficiarioEntradaVm vm, CancellationToken ct);
    Task InativarAsync(Guid id, CancellationToken ct);
    Task ExcluirAsync(Guid id, CancellationToken ct);
}
