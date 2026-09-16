using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class PlanoService(IEntityRepository<Plano> repository, IUnitOfWork work, IMapper mapper) : IPlanoService
{
    public async Task<List<PlanoRespostaVm>> ListarAsync(CancellationToken ct) =>
        mapper.Map<List<PlanoRespostaVm>>((await repository.ListarAsync(null, ct)).OrderBy(x => x.Nome));
    public async Task<PlanoRespostaVm> ObterAsync(Guid id, CancellationToken ct) =>
        mapper.Map<PlanoRespostaVm>(await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound));
    public async Task<PlanoRespostaVm> CriarAsync(PlanoEntradaVm vm, CancellationToken ct)
    {
        var entity = mapper.Map<Plano>(mapper.Map<PlanoVm>(vm));
        repository.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<PlanoRespostaVm>(entity);
    }
    public async Task AtualizarAsync(Guid id, PlanoEntradaVm vm, CancellationToken ct)
    {
        var entity = await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound);
        mapper.Map(mapper.Map<PlanoVm>(vm), entity);
        await work.SalvarAsync(ct);
    }
    public async Task SuspenderNotificacaoValidadeAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound);
        entity.SuspenderNotificacaoValidade();
        await work.SalvarAsync(ct);
    }
}
