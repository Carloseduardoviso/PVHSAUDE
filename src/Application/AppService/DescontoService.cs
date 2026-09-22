using AutoMapper;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;

namespace PVHSAUDE.Application.AppService;
public class DescontoService(IEntityRepository<Desconto> repository, IUnitOfWork work, IMapper mapper) : IDescontoService
{
    private static string NomeValido(string nome) => string.IsNullOrWhiteSpace(nome)
        ? throw new ServiceException(ServiceError.Invalid, "Informe o nome do desconto.")
        : nome.Trim();

    public async Task<List<DescontoRespostaVm>> ListarAsync(CancellationToken ct) =>
        mapper.Map<List<DescontoRespostaVm>>((await repository.ListarAsync(x => x.Ativo, ct)).OrderBy(x => x.Nome));

    public async Task<DescontoRespostaVm> ObterAsync(Guid id, CancellationToken ct) =>
        mapper.Map<DescontoRespostaVm>(await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound));

    public async Task<DescontoRespostaVm> CriarAsync(CatalogoEntradaVm vm, CancellationToken ct)
    {
        var nome = NomeValido(vm.Nome);
        if (await repository.ExisteAsync(x => x.Ativo && x.Nome.ToUpper() == nome.ToUpper(), ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe um desconto com este nome.");
        var entity = new Desconto(nome);
        repository.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<DescontoRespostaVm>(entity);
    }

    public async Task<DescontoRespostaVm> AtualizarAsync(Guid id, CatalogoEntradaVm vm, CancellationToken ct)
    {
        var nome = NomeValido(vm.Nome);
        var entity = await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound);
        if (await repository.ExisteAsync(x => x.Id != id && x.Ativo && x.Nome.ToUpper() == nome.ToUpper(), ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe um desconto com este nome.");
        entity.Atualizar(nome, vm.Ativo);
        await work.SalvarAsync(ct);
        return mapper.Map<DescontoRespostaVm>(entity);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken ct)
    {
        var entity = await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound);
        entity.Atualizar(entity.Nome, false);
        await work.SalvarAsync(ct);
    }
}
