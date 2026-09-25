using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Domain.Interfaces.Repository;

namespace PVHSAUDE.Application.AppService;

public class PortalAcessoService(
    IEntityRepository<Usuario> usuarios,
    IEntityRepository<Beneficiario> beneficiarios,
    IEntityRepository<Plano> planos,
    IUnitOfWork work,
    IPasswordHasher<Usuario> hasher) : IPortalAcessoService
{
    public async Task RegistrarAsync(CadastroAcessoBeneficiarioVm model, CancellationToken ct)
    {
        var cpf = Digitos(model.Cpf);
        if (cpf.Length != 11 || string.IsNullOrWhiteSpace(model.Senha))
            throw new ServiceException(ServiceError.Validation, "Informe CPF e senha válidos.");
        var titular = await EncontrarTitularDoCpfAsync(cpf, ct);
        var nomeCadastrado = titular.Cpf == cpf ? titular.Nome : titular.Dependentes.Single(x => x.Cpf == cpf).Nome;
        if (await usuarios.ExisteAsync(x => x.Role == Role.Beneficiario && x.CpfSolicitado == cpf && x.Ativo, ct))
            throw new ServiceException(ServiceError.Conflict, "Este CPF já possui acesso. Use a tela de login.", true);

        var usuarioId = Guid.NewGuid();
        var emailInterno = $"acesso-{usuarioId:N}@pvh.local";
        var conta = new Usuario
        {
            Id = usuarioId, NomeCompleto = nomeCadastrado, Email = emailInterno, EmailNormalizado = emailInterno.ToUpperInvariant(),
            CpfSolicitado = cpf, BeneficiarioId = titular.Id, Role = Role.Beneficiario, Ativo = true, MenusPermitidos = ""
        };
        conta.SenhaHash = hasher.HashPassword(conta, model.Senha);
        usuarios.Adicionar(conta);
        try { await work.SalvarAsync(ct); }
        catch (RegistroDuplicadoException)
        { throw new ServiceException(ServiceError.Conflict, "Este CPF já possui acesso.", true); }
    }

    public async Task<List<AcessoBeneficiarioPendenteVm>> ListarPendentesAsync(CancellationToken ct) =>
        (await usuarios.ListarAsync(x => x.Role == Role.Beneficiario && !x.Ativo && x.BeneficiarioId == null, ct))
        .OrderBy(x => x.NomeCompleto)
        .Select(x => new AcessoBeneficiarioPendenteVm(x.Id, x.NomeCompleto, x.Email, x.CpfSolicitado ?? ""))
        .ToList();

    public async Task AprovarAsync(Guid usuarioId, Guid beneficiarioId, CancellationToken ct)
    {
        var conta = await usuarios.ObterAsync(x => x.Id == usuarioId && x.Role == Role.Beneficiario, ct)
            ?? throw new ServiceException(ServiceError.NotFound);
        var beneficiario = await beneficiarios.ObterAsync(x => x.Id == beneficiarioId, ct, x => x.Dependentes)
            ?? throw new ServiceException(ServiceError.NotFound);
        if (conta.BeneficiarioId is not null || conta.Ativo ||
            Digitos(beneficiario.Cpf) != conta.CpfSolicitado && !beneficiario.Dependentes.Any(x => Digitos(x.Cpf) == conta.CpfSolicitado))
            throw new ServiceException(ServiceError.Validation, "O CPF da solicitação deve ser igual ao do beneficiário e a conta deve estar pendente.");
        if (await usuarios.ExisteAsync(x => x.BeneficiarioId == beneficiarioId && x.CpfSolicitado == conta.CpfSolicitado, ct))
            throw new ServiceException(ServiceError.Conflict, "Este beneficiário já possui acesso.", true);

        conta.BeneficiarioId = beneficiarioId;
        conta.Ativo = true;
        try { await work.SalvarAsync(ct); }
        catch (RegistroDuplicadoException)
        { throw new ServiceException(ServiceError.Conflict, "Este beneficiário já possui acesso.", true); }
    }

    public async Task<AcessoBeneficiarioEmitidoVm> EmitirAcessoAsync(string cpfInformado, CancellationToken ct)
    {
        var cpf = Digitos(cpfInformado);
        if (cpf.Length != 11) throw new ServiceException(ServiceError.Validation, "Informe um CPF completo.");
        var titular = await EncontrarTitularDoCpfAsync(cpf, ct);
        var pessoa = titular.Cpf == cpf ? titular.Nome : titular.Dependentes.Single(x => x.Cpf == cpf).Nome;
        var contas = await usuarios.ListarAsync(x => x.Role == Role.Beneficiario && x.CpfSolicitado == cpf, ct);
        if (contas.Any(x => x.Ativo && x.BeneficiarioId != titular.Id))
            throw new ServiceException(ServiceError.Conflict, "CPF já possui acesso em outro cadastro.", true);
        var conta = contas.FirstOrDefault(x => x.BeneficiarioId == titular.Id)
            ?? contas.FirstOrDefault(x => !x.Ativo && x.BeneficiarioId is null);
        if (conta is null)
        {
            var id = Guid.NewGuid();
            var emailInterno = $"acesso-{id:N}@pvh.local";
            conta = new Usuario
            {
                Id = id, NomeCompleto = pessoa, Email = emailInterno, EmailNormalizado = emailInterno.ToUpperInvariant(),
                CpfSolicitado = cpf, Role = Role.Beneficiario, MenusPermitidos = ""
            };
            usuarios.Adicionar(conta);
        }
        conta.NomeCompleto = pessoa;
        conta.BeneficiarioId = titular.Id;
        conta.Ativo = true;
        var senha = Convert.ToBase64String(RandomNumberGenerator.GetBytes(18)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        conta.SenhaHash = hasher.HashPassword(conta, senha);
        try { await work.SalvarAsync(ct); }
        catch (RegistroDuplicadoException)
        { throw new ServiceException(ServiceError.Conflict, "Já existe acesso para este CPF.", true); }
        return new AcessoBeneficiarioEmitidoVm(pessoa, cpf, senha);
    }

    public async Task<AreaBeneficiarioVm> MinhaAreaAsync(Guid usuarioId, CancellationToken ct)
    {
        var conta = await usuarios.ObterAsync(x => x.Id == usuarioId && x.Role == Role.Beneficiario && x.Ativo, ct);
        if (conta?.BeneficiarioId is not Guid beneficiarioId)
            throw new ServiceException(ServiceError.Unauthorized);
        var beneficiario = await beneficiarios.ObterAsync(x => x.Id == beneficiarioId, ct, x => x.Dependentes)
            ?? throw new ServiceException(ServiceError.NotFound);
        var cpfAcesso = conta.CpfSolicitado;
        var ehTitular = cpfAcesso == beneficiario.Cpf;
        var dependentesDoCpf = ehTitular ? new List<Dependente>() : beneficiario.Dependentes.Where(x => x.Cpf == cpfAcesso).ToList();
        if (!ehTitular && dependentesDoCpf.Count != 1) throw new ServiceException(ServiceError.Unauthorized);
        var dependente = dependentesDoCpf.FirstOrDefault();
        if (!ehTitular && dependente is null) throw new ServiceException(ServiceError.Unauthorized);
        var plano = await planos.ObterAsync(x => x.Id == beneficiario.PlanoId, ct);
        return new AreaBeneficiarioVm(
            dependente?.Id ?? beneficiario.Id, dependente?.Codigo ?? beneficiario.Codigo,
            dependente?.Nome ?? beneficiario.Nome, dependente?.Cpf ?? beneficiario.Cpf,
            dependente?.DataNascimento ?? beneficiario.DataNascimento,
            ehTitular ? beneficiario.Telefone : null, ehTitular ? beneficiario.Email : null, ehTitular ? beneficiario.Endereco : null,
            plano?.Nome ?? "Plano indisponível", ehTitular ? plano?.Valor : null, plano?.Periodicidade ?? Periodicidade.Mensal,
            beneficiario.DataInicio, beneficiario.DataValidade, beneficiario.Status,
            ehTitular ? beneficiario.Dependentes.Select(x => new AreaDependenteVm(x.Codigo, x.Nome, x.Cpf, x.DataNascimento, x.GrauParentesco, beneficiario.DataValidade)).ToList() : [],
            !ehTitular, dependente?.GrauParentesco, dependente is null ? null : Dependente.Valor);
    }

    private static string Digitos(string value) => new(value.Where(char.IsDigit).ToArray());

    private async Task<Beneficiario> EncontrarTitularDoCpfAsync(string cpf, CancellationToken ct)
    {
        var encontrados = await beneficiarios.ListarAsync(x => x.Cpf == cpf || x.Dependentes.Any(d => d.Cpf == cpf), ct, x => x.Dependentes);
        if (encontrados.Count == 0)
            throw new ServiceException(ServiceError.Validation, "Não existe carteirinha para o CPF informado. Antes de criar sua conta, solicite a carteirinha em 'Peça sua carteirinha'.");
        if (encontrados.Count != 1 || (encontrados[0].Cpf == cpf ? 1 : 0) + encontrados[0].Dependentes.Count(x => x.Cpf == cpf) != 1)
            throw new ServiceException(ServiceError.Conflict, "CPF encontrado em mais de um cadastro. Fale com a equipe da PVH Saúde.", true);
        return encontrados[0];
    }
}
