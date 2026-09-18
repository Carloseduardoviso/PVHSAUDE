using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Domain.Interfaces.Repository;
using System.Text.RegularExpressions;
namespace PVHSAUDE.Application.AppService;

public class BeneficiarioService(IEntityRepository<Beneficiario> repository, IEntityRepository<Credenciado> empresas,
    IEntityRepository<EmpresaBeneficiada> empresasBeneficiadas,
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
        if (vm.TipoPessoa == TipoPessoa.Juridica)
        {
            var empresa = await empresasBeneficiadas.ObterAsync(x => x.Id == vm.EmpresaBeneficiadaId, ct);
            if (empresa?.PlanoId is not Guid planoId)
                throw new ServiceException(ServiceError.Invalid, "Selecione uma empresa beneficiada com plano cadastrado.");
            vm.PlanoId = planoId;
        }
        else
        {
            vm.EmpresaBeneficiadaId = null;
            // Compatibilidade com beneficiários antigos que ainda enviam CredenciadoId.
            if (vm.CredenciadoId is Guid credenciadoId)
            {
                var empresaLegada = await empresas.ObterAsync(x => x.Id == credenciadoId, ct);
                if (empresaLegada?.PlanoId is Guid planoLegado) vm.PlanoId = planoLegado;
            }
            if (vm.PlanoId == Guid.Empty)
                throw new ServiceException(ServiceError.Invalid, "Selecione um plano para a pessoa física.");
        }
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
        var ano = DateTime.UtcNow.Year;
        var proximo = await ProximoNumeroAsync(ct);
        var entity = mapper.Map<Beneficiario>(Normalizar(vm));
        entity.DefinirCodigo(FormatarCodigo(proximo++, ano));
        entity.DefinirPessoa(vm.TipoPessoa, vm.EmpresaBeneficiadaId);
        Sincronizar(entity, vm.Dependentes, () => FormatarCodigo(proximo++, ano));
        repository.Adicionar(entity);
        await work.SalvarAsync(ct);
        return mapper.Map<BeneficiarioRespostaVm>(entity);
    }
    public async Task AtualizarAsync(Guid id, BeneficiarioEntradaVm vm, CancellationToken ct)
    {
        await ValidarPlano(vm, ct);
        var entity = await Encontrar(id, ct);
        var ano = DateTime.UtcNow.Year;
        var proximo = await ProximoNumeroAsync(ct);
        await ValidarDocumento(id, Digitos(vm.Cpf), ct);
        if (vm.Dependentes is { } lista &&
            (lista.Any(d => d.Id != Guid.Empty && !entity.Dependentes.Any(x => x.Id == d.Id)) ||
            lista.Where(d => d.Id != Guid.Empty).GroupBy(d => d.Id).Any(g => g.Count() > 1)))
            throw new ServiceException(ServiceError.Invalid, "Dependente inválido para este beneficiário.");
        Sincronizar(entity, vm.Dependentes, () => FormatarCodigo(proximo++, ano));
        mapper.Map(Normalizar(vm), entity);
        entity.DefinirPessoa(vm.TipoPessoa, vm.EmpresaBeneficiadaId);
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
    private async Task<int> ProximoNumeroAsync(CancellationToken ct)
    {
        var codigos = (await repository.ListarAsync(null, ct)).Select(x => x.Codigo)
            .Concat((await dependentes.ListarAsync(null, ct)).Select(x => x.Codigo));
        var maior = codigos.Select(ExtrairNumero).DefaultIfEmpty(0).Max();
        return maior + 1;
    }

    private static int ExtrairNumero(string? codigo) =>
        codigo is not null && Regex.Match(codigo, @"^RO(\d+)/\d{4}$") is { Success: true } m
            ? int.Parse(m.Groups[1].Value) : 0;

    private static string FormatarCodigo(int numero, int ano) => $"RO{numero:000}/{ano}";

    private void Sincronizar(Beneficiario entity, List<DependenteEntradaVm>? lista, Func<string> gerarCodigo)
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
            if (item.Id == Guid.Empty || string.Equals(dependente.Codigo, entity.Codigo, StringComparison.OrdinalIgnoreCase))
                dependente.DefinirCodigo(gerarCodigo());
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
