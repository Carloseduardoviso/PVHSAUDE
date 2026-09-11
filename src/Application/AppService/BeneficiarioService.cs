using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class BeneficiarioService(IEntityRepository<Beneficiario> repository, IEntityRepository<Credenciado> empresas,
    IEntityRepository<Dependente> dependentes, IUnitOfWork work, IMapper mapper) : IBeneficiarioService
{
    public async Task<List<BeneficiarioRespostaVm>> ListarAsync(CancellationToken ct) =>
        mapper.Map<List<BeneficiarioRespostaVm>>((await repository.ListarAsync(null, ct, x => x.Dependentes)).OrderBy(x => x.Nome));
    private async Task<Beneficiario> Encontrar(Guid id, CancellationToken ct) =>
        await repository.ObterAsync(x => x.Id == id, ct, x => x.Dependentes) ?? throw new ServiceException(ServiceError.NotFound);
    public async Task<BeneficiarioRespostaVm> ObterAsync(Guid id, CancellationToken ct) =>
        mapper.Map<BeneficiarioRespostaVm>(await Encontrar(id, ct));
    private async Task ValidarPlano(BeneficiarioEntradaVm vm, CancellationToken ct)
    {
        var empresa = await empresas.ObterAsync(x => x.Id == vm.CredenciadoId, ct);
        if (empresa?.PlanoId is not Guid planoId)
            throw new ServiceException(ServiceError.Invalid, "Selecione uma empresa com plano cadastrado.");
        vm.PlanoId = planoId;
        if (vm.DataValidade < vm.DataInicio)
            throw new ServiceException(ServiceError.Validation, "A validade do benefício deve ser posterior à data de início.");
    }
    private async Task ValidarDocumento(Guid? id, string cpf, CancellationToken ct)
    {
        if (await repository.ExisteAsync(x => x.Id != id && x.Cpf == cpf, ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe um beneficiário com este CPF/CNPJ.");
    }
    public async Task<BeneficiarioRespostaVm> CriarAsync(BeneficiarioEntradaVm vm, CancellationToken ct)
    {
        await ValidarPlano(vm, ct);
        await ValidarDocumento(null, Digitos(vm.Cpf), ct);
        if (vm.Dependentes?.Any(x => x.Id != Guid.Empty) == true)
            throw new ServiceException(ServiceError.Invalid, "Novos dependentes não devem possuir um identificador.");
        var entity = mapper.Map<Beneficiario>(Normalizar(vm));
        Sincronizar(entity, vm.Dependentes);
        repository.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<BeneficiarioRespostaVm>(entity);
    }
    public async Task AtualizarAsync(Guid id, BeneficiarioEntradaVm vm, CancellationToken ct)
    {
        await ValidarPlano(vm, ct);
        var entity = await Encontrar(id, ct);
        await ValidarDocumento(id, Digitos(vm.Cpf), ct);
        if (vm.Dependentes is { } lista &&
            (lista.Any(d => d.Id != Guid.Empty && !entity.Dependentes.Any(x => x.Id == d.Id)) ||
            lista.Where(d => d.Id != Guid.Empty).GroupBy(d => d.Id).Any(g => g.Count() > 1)))
            throw new ServiceException(ServiceError.Invalid, "Dependente inválido para este beneficiário.");
        Sincronizar(entity, vm.Dependentes);
        mapper.Map(Normalizar(vm), entity);
        await work.SalvarAsync(ct);
    }
    public async Task InativarAsync(Guid id, CancellationToken ct)
    {
        (await Encontrar(id, ct)).DefinirStatus(StatusBeneficiario.Inativo);
        await work.SalvarAsync(ct);
    }
    public async Task ExcluirAsync(Guid id, CancellationToken ct)
    {
        repository.Remover(await Encontrar(id, ct));
        await work.SalvarAsync(ct);
    }
    private void Sincronizar(Beneficiario entity, List<DependenteEntradaVm>? lista)
    {
        if (lista is null) return;
        foreach (var antigo in entity.Dependentes.Where(x => !lista.Any(d => d.Id == x.Id)).ToList())
        {
            dependentes.Remover(antigo);
            entity.Dependentes.Remove(antigo);
        }
        foreach (var item in lista)
        {
            var vm = mapper.Map<DependenteVm>(item);
            vm.Nome = vm.Nome.Trim();
            vm.Cpf = Digitos(vm.Cpf);
            var dependente = item.Id == Guid.Empty
                ? new Dependente(entity.Id, vm.Nome, vm.Cpf, vm.DataNascimento!.Value, vm.GrauParentesco!.Value)
                : entity.Dependentes.Single(x => x.Id == item.Id);
            mapper.Map(vm, dependente);
            if (item.Id == Guid.Empty) entity.Dependentes.Add(dependente);
        }
    }
    private BeneficiarioVm Normalizar(BeneficiarioEntradaVm entrada)
    {
        var vm = mapper.Map<BeneficiarioVm>(entrada);
        vm.Nome = vm.Nome.Trim();
        vm.Cpf = Digitos(vm.Cpf);
        vm.Telefone = vm.Telefone?.Trim();
        vm.Email = vm.Email?.Trim();
        vm.Endereco = vm.Endereco?.Trim();
        return vm;
    }
    private static string Digitos(string value) => new(value.Where(char.IsDigit).ToArray());
}
