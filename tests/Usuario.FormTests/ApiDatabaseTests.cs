using AutoMapper;
using Infra.Data.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Api.Controllers;
using PVHSAUDE.Application.AppService;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Infra.Data;

internal static class ApiDatabaseTests
{
    public static async Task Run(Context db, IMapper mapper)
    {
        var ct = CancellationToken.None;
        var work = new UnitOfWork(db);
        var planosRepo = new EntityRepository<Plano>(db);
        var empresasRepo = new EntityRepository<Credenciado>(db);
        var beneficiariosRepo = new EntityRepository<Beneficiario>(db);
        var dependentesRepo = new EntityRepository<Dependente>(db);
        void Check(bool ok, string message)
        { if (!ok) throw new Exception(message); Console.WriteLine("PASS: API em camadas - " + message); }
        var planos = new PlanosController(new PlanoService(planosRepo, work, mapper));
        var criado = (CreatedAtActionResult)await planos.Criar(new PlanoEntradaVm {
            Nome = " Plano teste ", Valor = 99.90m, Periodicidade = Periodicidade.Mensal }, ct);
        var plano = (PlanoRespostaVm)criado.Value!;
        Check(plano.Nome == "Plano teste" && criado.ActionName == "Obter", "plano mantém resposta 201 e normalização.");
        Check(await planos.Atualizar(plano.Id, new PlanoEntradaVm { Nome = "Plano atualizado", Valor = 120, Periodicidade = Periodicidade.Anual }, ct) is NoContentResult,
            "edição de plano retorna 204.");

        var catalogos = new CatalogoService(new EntityRepository<Especialidade>(db), new EntityRepository<Procedimento>(db), work, mapper);
        var especialidade = await catalogos.CriarEspecialidadeAsync(new CatalogoEntradaVm(" Especialidade "), ct);
        var procedimento = await catalogos.CriarProcedimentoAsync(new CatalogoEntradaVm(" Procedimento "), ct);
        Check((await catalogos.EspecialidadesAsync(ct)).Single().Nome == "Especialidade", "catálogo usa VM e entidade.");
        var empresaService = new CredenciadoService(empresasRepo, planosRepo, new EntityRepository<CredenciadoEspecialidade>(db),
            new EntityRepository<CredenciadoProcedimento>(db), work, mapper, new Storage());
        var empresas = new CredenciadosController(empresaService);
        var entrada = new CredenciadoEntradaVm { PlanoId = plano.Id, RazaoSocial = "Empresa", NomeFantasia = "Clínica",
            Cnpj = "12.345.678/0001-90", Tipo = TipoCredenciado.Clinica, StatusCredenciamento = StatusCredenciamento.Ativo,
            EspecialidadeIds = [especialidade.Id], ProcedimentoIds = [procedimento.Id] };
        var empresa = (CredenciadoRespostaVm)((CreatedAtActionResult)await empresas.Criar(entrada, ct)).Value!;
        Check(empresa.Cnpj == "12345678000190" && empresa.EspecialidadeIds.Single() == especialidade.Id, "credenciado retorna vínculos após gravar.");
        Check(await empresas.Atualizar(empresa.Id, entrada, ct) is NoContentResult, "atualização mantém vínculos sem conflito de rastreamento.");
        entrada.EspecialidadeIds = [];
        Check(await empresas.Atualizar(empresa.Id, entrada, ct) is NoContentResult && !await db.CredenciadoEspecialidades.AnyAsync(),
            "vínculos removidos são persistidos.");
        var portal = new PortalService(empresasRepo, mapper);
        Check((await portal.PlanosPorEmpresaAsync(ct)).Single().Valor == 120, "portal mantém valor do plano da empresa.");

        var service = new BeneficiarioService(beneficiariosRepo, empresasRepo, dependentesRepo, work, mapper);
        var controller = new BeneficiariosController(service);
        var request = new BeneficiarioEntradaVm { Nome = " Titular ", Cpf = "123.456.789-01", CredenciadoId = empresa.Id,
            PlanoId = Guid.NewGuid(), DataNascimento = new DateTime(1990, 1, 1), DataInicio = DateTime.Today,
            DataValidade = DateTime.Today.AddYears(1), Dependentes = [
                new DependenteEntradaVm { Nome = "Filho", Cpf = "98765432100", DataNascimento = new DateTime(2020, 1, 1), GrauParentesco = GrauParentesco.Filho }] };
        var beneficiario = (BeneficiarioRespostaVm)((CreatedAtActionResult)await controller.Criar(request, ct)).Value!;
        var dependenteId = beneficiario.Dependentes.Single().Id;
        Check(beneficiario.PlanoId == plano.Id && beneficiario.Cpf == "12345678901", "beneficiário recebe plano da empresa e documento normalizado.");
        Check(await controller.Criar(request, ct) is ConflictObjectResult, "CPF duplicado mantém conflito.");
        request.Dependentes = null;
        Check(await controller.Atualizar(beneficiario.Id, request, ct) is NoContentResult &&
            (await service.ObterAsync(beneficiario.Id, ct)).Dependentes.Single().Id == dependenteId, "dependentes omitidos são preservados.");
        request.Dependentes = [new DependenteEntradaVm { Id = Guid.NewGuid(), Nome = "Outro", Cpf = "12345678900",
            DataNascimento = new DateTime(2020, 1, 1), GrauParentesco = GrauParentesco.Filho }];
        Check(await controller.Atualizar(beneficiario.Id, request, ct) is BadRequestObjectResult, "dependente de outro titular é rejeitado.");
        request.Dependentes = [];
        Check(await controller.Atualizar(beneficiario.Id, request, ct) is NoContentResult && !await db.Dependentes.AnyAsync(),
            "lista vazia remove dependentes.");
        Check(await controller.Inativar(beneficiario.Id, ct) is NoContentResult &&
            (await service.ObterAsync(beneficiario.Id, ct)).Status == StatusBeneficiario.Inativo, "inativação preserva cadastro.");
        Check(await controller.Excluir(beneficiario.Id, ct) is NoContentResult &&
            await controller.Obter(beneficiario.Id, ct) is NotFoundResult, "exclusão retorna 204 e consulta posterior retorna 404.");

        var contatos = new ContatoService(new EntityRepository<Contato>(db), work, mapper);
        await contatos.CriarAsync(new ContatoEntradaVm { Nome = "Teste", Email = "teste@example.com" }, ct);
        var contato = (await contatos.ListarAsync(ct)).Single();
        Check(contato.Id != Guid.Empty && contato.EnviadoEm > DateTime.UtcNow.AddMinutes(-5), "contato recebe ID e data do servidor.");
        await contatos.ExcluirAsync(contato.Id, ct);
        Check((await contatos.ListarAsync(ct)).Count == 0, "mensagem removida pelo repository.");
    }

    private sealed class Storage : IImagemStorage
    {
        public Task<string> SalvarCredenciadoAsync(Guid id, string extensao, Stream conteudo, CancellationToken ct) =>
            throw new NotSupportedException("Este teste não grava arquivos.");
    }
}
