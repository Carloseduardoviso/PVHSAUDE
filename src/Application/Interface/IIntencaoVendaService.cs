using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Application.Interface;

public interface IIntencaoVendaService
{
    Task<List<IntencaoVendaVm>> ListarAsync(CancellationToken ct);
    Task CriarAsync(IntencaoVendaEntradaVm vm, CancellationToken ct);
    Task AtualizarStatusAsync(Guid id, StatusIntencaoVenda status, CancellationToken ct);
}
