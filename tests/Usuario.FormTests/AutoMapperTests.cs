using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using Microsoft.Extensions.Logging.Abstractions;
using PVHSAUDE.Application.AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using System.Linq.Expressions;

internal static class AutoMapperTests
{
    public static void Run()
    {
        var config = new MapperConfiguration(c =>
        {
            c.AddExpressionMapping();
            c.AddProfile<AutoMapperConfig>();
        }, NullLoggerFactory.Instance);
        config.AssertConfigurationIsValid();
        var mapper = config.CreateMapper();
        void Check(bool value, string message)
        {
            if (!value) throw new Exception(message);
            Console.WriteLine("PASS: AutoMapper - " + message);
        }

        var plano = new Plano("Original", null, 10, (Periodicidade)1, null);
        var planoId = plano.Id;
        var mappedPlano = mapper.Map(new PlanoVm { Id = Guid.NewGuid(), Nome = " Novo ", Descricao = " Texto ",
            Valor = 25, Periodicidade = (Periodicidade)1, DataValidade = new DateTime(2030, 1, 2, 15, 0, 0) }, plano);
        Check(ReferenceEquals(mappedPlano, plano) && plano.Id == planoId && plano.Nome == "Novo"
            && plano.Descricao == "Texto" && plano.DataValidade == new DateTime(2030, 1, 2), "edição preserva ID e normalização do plano.");
        Check(mapper.Map<PlanoVm>(plano).Valor == 25, "plano retorna VM.");
        Expression<Func<PlanoVm, bool>> filtro = vm => vm.Valor > 20;
        Check(mapper.Map<Expression<Func<Plano, bool>>>(filtro).Compile()(plano), "filtro VM é convertido para entidade.");

        var beneficiario = new Beneficiario("Titular", "123", DateTime.Today.AddYears(-30), planoId, DateTime.Today, DateTime.Today.AddYears(1));
        var dependente = new Dependente(beneficiario.Id, "Filho", "456", DateTime.Today.AddYears(-5), (GrauParentesco)1);
        beneficiario.Dependentes.Add(dependente);
        var id = beneficiario.Id;
        var adesao = beneficiario.DataAdesao;
        var vm = mapper.Map<BeneficiarioVm>(beneficiario);
        Check(vm.Dependentes.Count == 1 && vm.Dependentes[0].Id == dependente.Id, "dependentes são mapeados na leitura.");
        vm.Id = Guid.NewGuid();
        vm.Dependentes.Clear();
        mapper.Map(vm, beneficiario);
        Check(beneficiario.Id == id && beneficiario.DataAdesao == adesao && beneficiario.Dependentes.Single() == dependente,
            "edição preserva identidade, adesão e dependentes existentes.");
        var dependenteId = dependente.Id;
        mapper.Map(new DependenteVm { Id = Guid.NewGuid(), Nome = "Atualizado", Cpf = "789",
            DataNascimento = DateTime.Today.AddYears(-6), GrauParentesco = (GrauParentesco)1 }, dependente);
        Check(dependente.Id == dependenteId && dependente.BeneficiarioId == id && dependente.Nome == "Atualizado", "dependente mantém seu titular.");

        var empresaVm = new CredenciadoVm { NomeFantasia = " Clínica ", RazaoSocial = " Empresa ", Cnpj = "12.345.678/0001-90",
            Uf = " am ", PlanoId = planoId, Tipo = (TipoCredenciado)1, StatusCredenciamento = StatusCredenciamento.Ativo };
        var empresa = mapper.Map<Credenciado>(empresaVm);
        empresa.DefinirImagem("/imagem-existente.png");
        var especialidade = new Especialidade(" Clínica ");
        var procedimento = new Procedimento(" Exame ");
        var vinculo = new CredenciadoEspecialidade(empresa.Id, especialidade.Id);
        empresa.Especialidades.Add(vinculo);
        empresa.Procedimentos.Add(new CredenciadoProcedimento(empresa.Id, procedimento.Id));
        var leitura = mapper.Map<CredenciadoVm>(empresa);
        Check(leitura.EspecialidadeIds.Single() == especialidade.Id && leitura.ProcedimentoIds.Single() == procedimento.Id, "IDs dos vínculos são mapeados.");
        mapper.Map(empresaVm, empresa);
        Check(empresa.Cnpj == "12345678000190" && empresa.Uf == "AM" && empresa.ImagemUrl == "/imagem-existente.png"
            && empresa.Especialidades.Single() == vinculo, "credenciado mantém imagem, vínculos e normalização.");
        Check(mapper.Map<CredenciadoEspecialidade>(mapper.Map<CredenciadoEspecialidadeVm>(vinculo)).EspecialidadeId == especialidade.Id, "vínculo de especialidade.");
        var procedimentoVinculo = empresa.Procedimentos.Single();
        Check(mapper.Map<CredenciadoProcedimento>(mapper.Map<CredenciadoProcedimentoVm>(procedimentoVinculo)).ProcedimentoId == procedimento.Id, "vínculo de procedimento.");
        Check(mapper.Map<Especialidade>(mapper.Map<EspecialidadeVm>(especialidade)).Nome == "Clínica", "especialidade.");
        Check(mapper.Map<Procedimento>(mapper.Map<ProcedimentoVm>(procedimento)).Nome == "Exame", "procedimento.");

        var banner = new Banner { Imagem = [1, 2, 3], ContentType = "image/png" };
        var bannerId = banner.Id;
        var criado = banner.CriadoEm;
        mapper.Map(new BannerVm { Id = Guid.NewGuid(), Titulo = " Banner ", Ativo = false }, banner);
        Check(banner.Id == bannerId && banner.CriadoEm == criado && banner.Imagem.Length == 3 && banner.ContentType == "image/png"
            && mapper.Map<BannerVm>(banner).Titulo == "Banner", "banner preserva arquivo e metadados.");
        var contato = new Contato();
        var contatoId = contato.Id;
        var enviado = contato.EnviadoEm;
        mapper.Map(new ContatoVm { Id = Guid.NewGuid(), Nome = "Contato", EnviadoEm = DateTime.MinValue }, contato);
        Check(contato.Id == contatoId && contato.EnviadoEm == enviado && mapper.Map<ContatoVm>(contato).Nome == "Contato", "contato preserva ID e data.");

        var usuario = mapper.Map<Usuario>(new UsuarioCadastroVm { NomeCompleto = " Nome ", Email = " Pessoa@exemplo.com ",
            Senha = "nao-mapear", Menus = ["Plano"] });
        usuario.SenhaHash = "hash-existente";
        usuario.Ativo = false;
        var usuarioId = usuario.Id;
        mapper.Map(new UsuarioEdicaoVm { UsuarioId = Guid.NewGuid(), NomeCompleto = " Editado ", Email = " Outro@exemplo.com ",
            Senha = "tambem-nao-mapear", Menus = ["Beneficiario"] }, usuario);
        var usuarioVm = mapper.Map<UsuarioVm>(usuario);
        Check(usuario.Id == usuarioId && !usuario.Ativo && usuario.SenhaHash == "hash-existente"
            && usuario.EmailNormalizado == "OUTRO@EXEMPLO.COM" && usuarioVm.UsuarioId == usuarioId
            && usuarioVm.Menus.Single() == "Beneficiario", "usuário preserva ID, status e hash, normaliza e-mail e converte menus.");
        Check(mapper.Map<UsuarioEdicaoVm>(usuario).Senha is null, "senha nunca retorna na VM.");
        Console.WriteLine("PASS: configuração de todos os mapeamentos válida.");
    }
}