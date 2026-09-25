using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using PVHSAUDE.Application.AppService;
using PVHSAUDE.Application.AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Api.Controllers;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Domain.Interfaces.Repository;
using PVHSAUDE.Infra.Auth.Interface;
using PVHSAUDE.Infra.Helper.Settings;

internal static class AreaBeneficiarioTests
{
    public static async Task Run()
    {
        var usuarios = new MemoryRepository<Usuario>();
        var beneficiarios = new MemoryRepository<Beneficiario>();
        var planos = new MemoryRepository<Plano>();
        var hasher = new PasswordHasher<Usuario>();
        var service = new PortalAcessoService(usuarios, beneficiarios, planos, new MemoryWork(), hasher);
        var plano = new Plano("Família", null, 49.90m, Periodicidade.Mensal, null);
        var intencao = new IntencaoVenda("Ana Silva", "ana@example.com", "69999999999", "12345678901",
            plano.Id, plano.Nome, 49.90m, 2, "Endereço", "Bruno; Maria");
        Check(intencao.ValorTotal == 73.70m, "solicitação calcula R$ 11,90 por dependente");
        planos.Adicionar(plano);
        var titular = new Beneficiario("Ana Silva", "12345678901", new DateTime(1990, 1, 1), plano.Id, DateTime.Today, DateTime.Today.AddYears(1));
        titular.Dependentes.Add(new Dependente(titular.Id, "Bruno Silva", "55544433322", new DateTime(2012, 2, 2), GrauParentesco.Filho));
        beneficiarios.Adicionar(titular);

        try
        {
            await service.RegistrarAsync(new CadastroAcessoBeneficiarioVm
            {
                Cpf = "99988877766", Senha = "senha-segura-123"
            }, CancellationToken.None);
            throw new Exception("CPF sem carteirinha criou conta.");
        }
        catch (ServiceException ex) when (ex.Error == ServiceError.Validation && ex.Message.Contains("Peça sua carteirinha")) { }
        Check(usuarios.Items.Count == 0, "CPF sem carteirinha não cria conta e recebe orientação");

        await service.RegistrarAsync(new CadastroAcessoBeneficiarioVm
        {
            Cpf = "123.456.789-01", Senha = "senha-segura-123"
        }, CancellationToken.None);
        var conta = usuarios.Items.Single();
        Check(conta.Role == Role.Beneficiario && conta.Ativo && conta.BeneficiarioId == titular.Id && conta.CpfSolicitado == titular.Cpf,
            "titular cadastrado na administração recebe conta vinculada");
        Check(conta.NomeCompleto == titular.Nome && conta.Email.StartsWith("acesso-") && conta.Email.EndsWith("@pvh.local"),
            "nome vem do cadastro administrativo e e-mail interno não é solicitado ao cliente");
        Check(hasher.VerifyHashedPassword(conta, conta.SenhaHash, "senha-segura-123") != PasswordVerificationResult.Failed,
            "senha do titular é armazenada como hash verificável");
        var area = await service.MinhaAreaAsync(conta.Id, CancellationToken.None);
        Check(area.Nome == "Ana Silva" && area.PlanoNome == "Família" && area.Dependentes.Count == 1,
            "titular acessa seus dados, plano e dependentes");
        Check(!area.EhDependente, "carteirinha do titular recebe identificação correta");
        Check(area.PlanoValor == 49.90m, "titular consulta o valor do próprio plano");

        await service.RegistrarAsync(new CadastroAcessoBeneficiarioVm
        {
            Cpf = "555.444.333-22", Senha = "senha-segura-456"
        }, CancellationToken.None);
        var contaDependente = usuarios.Items.Single(x => x.CpfSolicitado == "55544433322");
        var areaDependente = await service.MinhaAreaAsync(contaDependente.Id, CancellationToken.None);
        Check(contaDependente.BeneficiarioId == titular.Id && areaDependente.Nome == "Bruno Silva" && areaDependente.Cpf == "55544433322",
            "CPF do dependente também cria acesso ao plano da família");
        Check(areaDependente.Dependentes.Count == 0 && areaDependente.Email is null && areaDependente.Endereco is null,
            "dependente não recebe dados pessoais dos outros membros");
        Check(areaDependente.EhDependente, "carteirinha do dependente recebe identificação correta");
        Check(areaDependente.PlanoValor is null && areaDependente.GrauParentesco == GrauParentesco.Filho,
            "dependente recebe parentesco sem o preço do plano do titular");
        Check(areaDependente.ValorDependente == 11.90m,
            "dependente consulta o valor mostrado no cadastro administrativo");

        try
        {
            await service.RegistrarAsync(new CadastroAcessoBeneficiarioVm
            {
                Cpf = titular.Cpf, Senha = "senha-segura-789"
            }, CancellationToken.None);
            throw new Exception("CPF com conta existente criou duplicata.");
        }
        catch (ServiceException ex) when (ex.Error == ServiceError.Conflict) { }
        Check(usuarios.Items.Count == 2, "CPF já vinculado não cria outra conta");

        var mapper = new MapperConfiguration(c => { c.AddExpressionMapping(); c.AddProfile<AutoMapperConfig>(); }, NullLoggerFactory.Instance).CreateMapper();
        var jwt = new AppJwtService(Options.Create(new JwtSetting { SecretKey = new string('x', 64), Issuer = "tests", Audience = "tests" }));
        var auth = new AuthService(usuarios, beneficiarios, new MemoryWork(), mapper, hasher, jwt);
        var entradaTitular = await auth.LoginCpfAsync(new LoginCpfBeneficiarioVm { Cpf = "123.456.789-01", Senha = "senha-segura-123" }, CancellationToken.None);
        var entradaDependente = await auth.LoginCpfAsync(new LoginCpfBeneficiarioVm { Cpf = "55544433322", Senha = "senha-segura-456" }, CancellationToken.None);
        Check(entradaTitular.Usuario.UsuarioId == conta.Id && entradaDependente.Usuario.UsuarioId == contaDependente.Id,
            "login com CPF e senha identifica titular e dependente separadamente");
        try
        {
            await auth.LoginCpfAsync(new LoginCpfBeneficiarioVm { Cpf = titular.Cpf, Senha = "senha-errada" }, CancellationToken.None);
            throw new Exception("Senha incorreta deu acesso.");
        }
        catch (ServiceException ex) when (ex.Error == ServiceError.Unauthorized) { }
        Check(true, "senha incorreta não dá acesso");

        beneficiarios.Adicionar(new Beneficiario("Sem Conta", "98765432100", new DateTime(1994, 1, 1), plano.Id, DateTime.Today, DateTime.Today.AddYears(1)));
        var endpoint = new AuthController(auth);
        Check(await endpoint.LoginBeneficiario(new LoginCpfBeneficiarioVm { Cpf = "98765432100", Senha = "qualquer-senha" }, CancellationToken.None) is NotFoundResult,
            "CPF com carteirinha e sem conta recebe resposta de cadastro ausente");
        Check(await endpoint.LoginBeneficiario(new LoginCpfBeneficiarioVm { Cpf = titular.Cpf, Senha = "senha-errada" }, CancellationToken.None) is UnauthorizedResult,
            "senha incorreta mantém resposta de credenciais inválidas");
    }

    private static void Check(bool ok, string message)
    {
        if (!ok) throw new Exception("Área do beneficiário: " + message);
        Console.WriteLine("PASS: Área do beneficiário - " + message);
    }

    private sealed class MemoryRepository<T> : IEntityRepository<T> where T : class
    {
        public List<T> Items { get; } = [];
        public Task<List<T>> ListarAsync(Expression<Func<T, bool>>? filtro, CancellationToken ct, params Expression<Func<T, object>>[] includes)
            => Task.FromResult((filtro is null ? Items : Items.Where(filtro.Compile())).ToList());
        public Task<T?> ObterAsync(Expression<Func<T, bool>> filtro, CancellationToken ct, params Expression<Func<T, object>>[] includes)
            => Task.FromResult(Items.SingleOrDefault(filtro.Compile()));
        public Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro, CancellationToken ct)
            => Task.FromResult(Items.Any(filtro.Compile()));
        public void Adicionar(T entidade) => Items.Add(entidade);
        public void Remover(T entidade) => Items.Remove(entidade);
    }

    private sealed class MemoryWork : IUnitOfWork
    {
        public Task SalvarAsync(CancellationToken ct) => Task.CompletedTask;
        public Task<IRepositoryTransaction> BloquearUsuariosAsync(CancellationToken ct) => throw new NotSupportedException();
    }
}
