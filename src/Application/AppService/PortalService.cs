using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class PortalService(IEntityRepository<Credenciado> repository, IMapper mapper) : IPortalService
{
    public async Task<List<EmpresaPlanoVm>> PlanosPorEmpresaAsync(CancellationToken ct) =>
        mapper.Map<List<EmpresaPlanoVm>>(await repository.ListarAsync(
            x => x.StatusCredenciamento == StatusCredenciamento.Ativo && x.Plano != null, ct, x => x.Plano!))
            .Distinct().OrderBy(x => x.Plano).ThenBy(x => x.PlanoId).ToList();
}
