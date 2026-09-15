using AutoMapper;
using Infra.Data.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using PVHSAUDE.Application.AppService;
using PVHSAUDE.Application.AutoMapper;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Infra.Auth.Interface;
using PVHSAUDE.Infra.Data;
using PVHSAUDE.Infra.Ioc;

internal static class DatabaseTests
{
    public static async Task Run()
    {
        var database = "PVHSAUDE_EmpresaBeneficiadaTests_" + Guid.NewGuid().ToString("N");
        using var services = new ServiceCollection().AddLogging().AddHttpContextAccessor().AddHttpClient().AddInfrastructure().BuildServiceProvider();
        var options = new DbContextOptionsBuilder<Context>().UseSqlServer(
            $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=true;TrustServerCertificate=true").Options;
        await using var db = new Context(options, services.GetRequiredService<IAccount>());
        var config = new MapperConfiguration(c => c.AddProfile<EmpresaBeneficiadaProfile>(), NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
        var mapper = config.CreateMapper();
        var storage = new Storage();
        var service = new EmpresaBeneficiadaService(new EntityRepository<EmpresaBeneficiada>(db), new EntityRepository<Plano>(db),
            new EntityRepository<EmpresaBeneficiadaEspecialidade>(db), new EntityRepository<EmpresaBeneficiadaProcedimento>(db), new UnitOfWork(db), mapper, storage);
        try
        {
            await db.Database.MigrateAsync();
            Check(!(await db.Database.GetPendingMigrationsAsync()).Any(), "Migrações aplicadas no banco isolado");
            var plano = new Plano("Plano teste", null, 99, (Periodicidade)1, null);
            var especialidade = new Especialidade("Especialidade teste");
            var procedimento = new Procedimento("Procedimento teste");
            var clinica = new Credenciado("Clínica", "Clínica existente", "12345678000190", null, null, null, null, null, null, null, null, TipoCredenciado.Clinica, (StatusCredenciamento)1);
            clinica.DefinirPlano(plano.Id);
            db.AddRange(plano, especialidade, procedimento, clinica);
            await db.SaveChangesAsync();
            var vm = new EmpresaBeneficiadaEntradaVm
            {
                RazaoSocial = " Empresa teste ", NomeFantasia = " Beneficiada teste ", Cnpj = "12.345.678/0001-90",
                PlanoId = plano.Id, Tipo = TipoCredenciado.Outro, StatusCredenciamento = (StatusCredenciamento)1,
                EspecialidadeIds = [especialidade.Id, especialidade.Id], ProcedimentoIds = [procedimento.Id]
            };
            var created = await service.CriarAsync(vm, default);
            Check(created.Id != Guid.Empty && created.Cnpj == "12345678000190" && created.NomeFantasia == "Beneficiada teste", "Criação normaliza dados e retorna ID");
            db.ChangeTracker.Clear();
            var read = await service.ObterAsync(created.Id, default);
            Check(read.PlanoId == plano.Id && read.EspecialidadeIds.SequenceEqual([especialidade.Id]) && read.ProcedimentoIds.SequenceEqual([procedimento.Id]), "Plano e catálogos persistem sem duplicatas");
            Check((await service.ListarAsync(default)).Count == 1 && await db.Set<Credenciado>().CountAsync() == 1, "Empresas e clínicas possuem cadastros separados, inclusive com o mesmo CNPJ");
            await Reject(() => service.CriarAsync(vm, default), "CNPJ duplicado é rejeitado no cadastro de empresas");
            vm.PlanoId = Guid.NewGuid();
            await Reject(() => service.CriarAsync(vm, default), "Plano inexistente é rejeitado");
            vm.PlanoId = plano.Id;
            vm.NomeFantasia = "Alterada";
            vm.EspecialidadeIds = [];
            vm.ProcedimentoIds = [];
            await service.AtualizarAsync(created.Id, vm, default);
            db.ChangeTracker.Clear();
            read = await service.ObterAsync(created.Id, default);
            Check(read.NomeFantasia == "Alterada" && read.EspecialidadeIds.Count == 0 && read.ProcedimentoIds.Count == 0, "Edição altera os dados e remove vínculos desmarcados");
            Check((await db.Set<Credenciado>().SingleAsync()).NomeFantasia == "Clínica existente", "Edição da empresa preserva a clínica");
            await service.UploadImagemAsync(created.Id, "empresa.png", 3, new MemoryStream([1, 2, 3]), default);
            db.ChangeTracker.Clear();
            Check((await service.ObterAsync(created.Id, default)).ImagemUrl == $"/uploads/credenciados/{created.Id}.png" && storage.Id == created.Id, "Imagem fica vinculada à empresa correta");
            await Reject(() => service.UploadImagemAsync(created.Id, "empresa.exe", 3, Stream.Null, default), "Formato de imagem inválido é rejeitado");
            await service.UploadImagemAsync(created.Id, "limite.png", 5_242_880, Stream.Null, default);
            await Reject(() => service.UploadImagemAsync(created.Id, "grande.png", 5_242_881, Stream.Null, default), "Imagem acima de 5 MB é rejeitada");
            await Reject(() => service.ObterAsync(clinica.Id, default), "ID de clínica não pode abrir empresa beneficiada");
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }

    private static void Check(bool ok, string message) { if (!ok) throw new Exception(message); Console.WriteLine("PASS: " + message); }
    private static async Task Reject(Func<Task> action, string message)
    {
        try { await action(); }
        catch (ServiceException) { Console.WriteLine("PASS: " + message); return; }
        throw new Exception(message);
    }
    private sealed class Storage : IImagemStorage
    {
        public Guid Id;
        public Task<string> SalvarCredenciadoAsync(Guid id, string extensao, Stream conteudo, CancellationToken ct)
        { Id = id; return Task.FromResult($"/uploads/credenciados/{id}{extensao}"); }
    }
}
