using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class CatalogoService(IEntityRepository<Especialidade> especialidades, IEntityRepository<Procedimento> procedimentos, IUnitOfWork work, IMapper mapper) : ICatalogoService
{
    private static string NomeValido(string nome) => string.IsNullOrWhiteSpace(nome)
        ? throw new ServiceException(ServiceError.Invalid, "Informe o nome do catálogo.")
        : nome.Trim();

    public async Task<List<EspecialidadeRespostaVm>> EspecialidadesAsync(CancellationToken ct) =>
        mapper.Map<List<EspecialidadeRespostaVm>>((await especialidades.ListarAsync(x => x.Ativo, ct)).OrderBy(x => x.Nome));
    public async Task<List<ProcedimentoRespostaVm>> ProcedimentosAsync(CancellationToken ct) =>
        mapper.Map<List<ProcedimentoRespostaVm>>((await procedimentos.ListarAsync(x => x.Ativo, ct)).OrderBy(x => x.Nome));
    public async Task<EspecialidadeRespostaVm> CriarEspecialidadeAsync(CatalogoEntradaVm vm, CancellationToken ct)
    {
        var nome = NomeValido(vm.Nome);
        if (await especialidades.ExisteAsync(x => x.Ativo && x.Nome.ToUpper() == nome.ToUpper(), ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe uma especialidade com este nome.");
        var entity = mapper.Map<Especialidade>(new EspecialidadeVm { Nome = nome });
        especialidades.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<EspecialidadeRespostaVm>(entity);
    }
    public async Task<ProcedimentoRespostaVm> CriarProcedimentoAsync(CatalogoEntradaVm vm, CancellationToken ct)
    {
        var nome = NomeValido(vm.Nome);
        if (await procedimentos.ExisteAsync(x => x.Ativo && x.Nome.ToUpper() == nome.ToUpper(), ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe um procedimento com este nome.");
        var entity = mapper.Map<Procedimento>(new ProcedimentoVm { Nome = nome });
        procedimentos.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<ProcedimentoRespostaVm>(entity);
    }

    public async Task<EspecialidadeRespostaVm> AtualizarEspecialidadeAsync(Guid id, CatalogoEntradaVm vm, CancellationToken ct)
    {
        var nome = NomeValido(vm.Nome);
        var entity = await especialidades.ObterAsync(x => x.Id == id, ct)
            ?? throw new ServiceException(ServiceError.NotFound);
        if (await especialidades.ExisteAsync(x => x.Id != id && x.Ativo && x.Nome.ToUpper() == nome.ToUpper(), ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe uma especialidade com este nome.");
        entity.Atualizar(nome, vm.Ativo);
        await work.SalvarAsync(ct);
        return mapper.Map<EspecialidadeRespostaVm>(entity);
    }

    public async Task<ProcedimentoRespostaVm> AtualizarProcedimentoAsync(Guid id, CatalogoEntradaVm vm, CancellationToken ct)
    {
        var nome = NomeValido(vm.Nome);
        var entity = await procedimentos.ObterAsync(x => x.Id == id, ct)
            ?? throw new ServiceException(ServiceError.NotFound);
        if (await procedimentos.ExisteAsync(x => x.Id != id && x.Ativo && x.Nome.ToUpper() == nome.ToUpper(), ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe um procedimento com este nome.");
        entity.Atualizar(nome, vm.Ativo);
        await work.SalvarAsync(ct);
        return mapper.Map<ProcedimentoRespostaVm>(entity);
    }

    public async Task ExcluirEspecialidadeAsync(Guid id, CancellationToken ct)
    {
        var entity = await especialidades.ObterAsync(x => x.Id == id, ct)
            ?? throw new ServiceException(ServiceError.NotFound);
        entity.Atualizar(entity.Nome, false);
        await work.SalvarAsync(ct);
    }

    public async Task ExcluirProcedimentoAsync(Guid id, CancellationToken ct)
    {
        var entity = await procedimentos.ObterAsync(x => x.Id == id, ct)
            ?? throw new ServiceException(ServiceError.NotFound);
        entity.Atualizar(entity.Nome, false);
        await work.SalvarAsync(ct);
    }
}
