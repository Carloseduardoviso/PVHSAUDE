using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class ContatoService(IEntityRepository<Contato> repository, IUnitOfWork work, IMapper mapper) : IContatoService
{
    public async Task<List<ContatoVm>> ListarAsync(CancellationToken ct) =>
        mapper.Map<List<ContatoVm>>((await repository.ListarAsync(null, ct)).OrderByDescending(x => x.EnviadoEm));
    public async Task<ContatoVm> ObterAsync(Guid id, CancellationToken ct) =>
        mapper.Map<ContatoVm>(await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound));
    public async Task CriarAsync(ContatoEntradaVm vm, CancellationToken ct)
    {
        repository.Adicionar(mapper.Map<Contato>(vm));
        await work.SalvarAsync(ct);
    }
    public async Task ExcluirAsync(Guid id, CancellationToken ct)
    {
        repository.Remover(await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound));
        await work.SalvarAsync(ct);
    }
}
