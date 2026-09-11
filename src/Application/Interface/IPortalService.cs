using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IPortalService
{
    Task<List<EmpresaPlanoVm>> PlanosPorEmpresaAsync(CancellationToken ct);
}
