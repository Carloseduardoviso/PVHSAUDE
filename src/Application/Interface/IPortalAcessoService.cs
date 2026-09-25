using PVHSAUDE.Application.ViewModels;

namespace PVHSAUDE.Application.Interface;

public interface IPortalAcessoService
{
    Task RegistrarAsync(CadastroAcessoBeneficiarioVm model, CancellationToken ct);
    Task<List<AcessoBeneficiarioPendenteVm>> ListarPendentesAsync(CancellationToken ct);
    Task AprovarAsync(Guid usuarioId, Guid beneficiarioId, CancellationToken ct);
    Task<AcessoBeneficiarioEmitidoVm> EmitirAcessoAsync(string cpf, CancellationToken ct);
    Task<AreaBeneficiarioVm> MinhaAreaAsync(Guid usuarioId, CancellationToken ct);
}
