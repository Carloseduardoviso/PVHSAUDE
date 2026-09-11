using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class CatalogoService(IEntityRepository<Especialidade> especialidades, IEntityRepository<Procedimento> procedimentos, IUnitOfWork work, IMapper mapper) : ICatalogoService
{
    public async Task<List<EspecialidadeRespostaVm>> EspecialidadesAsync(CancellationToken ct) =>
        mapper.Map<List<EspecialidadeRespostaVm>>((await especialidades.ListarAsync(x => x.Ativo, ct)).OrderBy(x => x.Nome));
    public async Task<List<ProcedimentoRespostaVm>> ProcedimentosAsync(CancellationToken ct) =>
        mapper.Map<List<ProcedimentoRespostaVm>>((await procedimentos.ListarAsync(x => x.Ativo, ct)).OrderBy(x => x.Nome));
    public async Task<EspecialidadeRespostaVm> CriarEspecialidadeAsync(CatalogoEntradaVm vm, CancellationToken ct)
    {
        var entity = mapper.Map<Especialidade>(new EspecialidadeVm { Nome = vm.Nome });
        especialidades.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<EspecialidadeRespostaVm>(entity);
    }
    public async Task<ProcedimentoRespostaVm> CriarProcedimentoAsync(CatalogoEntradaVm vm, CancellationToken ct)
    {
        var entity = mapper.Map<Procedimento>(new ProcedimentoVm { Nome = vm.Nome });
        procedimentos.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<ProcedimentoRespostaVm>(entity);
    }
}
