using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;

namespace PVHSAUDE.Application.AutoMapper;

public class AutoMapperConfig : Profile
{
    public AutoMapperConfig()
    {
        CreateMap<PlanoEntradaVm, PlanoVm>().ForMember(x => x.Id, o => o.Ignore());
        CreateMap<CredenciadoEntradaVm, CredenciadoVm>()
            .ForMember(x => x.Id, o => o.Ignore())
            .ForMember(x => x.ImagemUrl, o => o.Ignore())
            .ForMember(x => x.ImagemUrls, o => o.Ignore());
        CreateMap<DependenteEntradaVm, DependenteVm>();
        CreateMap<BeneficiarioEntradaVm, BeneficiarioVm>().ForMember(x => x.Id, o => o.Ignore());
        CreateMap<Plano, PlanoRespostaVm>();
        CreateMap<Desconto, PlanoRespostaVm>();
        CreateMap<PlanoVm, Desconto>().ConvertUsing((vm, entity, _) =>
        {
            entity ??= new Desconto(vm.Nome, vm.Descricao, vm.Valor, vm.Periodicidade, vm.DataValidade, vm.TipoPessoa);
            entity.Atualizar(vm.Nome, vm.Descricao, vm.Valor, vm.Periodicidade, vm.DataValidade, vm.TipoPessoa);
            return entity;
        });
        CreateMap<Dependente, DependenteRespostaVm>();
        CreateMap<Beneficiario, BeneficiarioRespostaVm>();
        CreateMap<Especialidade, EspecialidadeRespostaVm>();
        CreateMap<Procedimento, ProcedimentoRespostaVm>();
        CreateMap<Credenciado, CredenciadoRespostaVm>()
            .ForCtorParam("EspecialidadeIds", o => o.MapFrom(e => e.Especialidades.Select(x => x.EspecialidadeId).ToList()))
            .ForCtorParam("ProcedimentoIds", o => o.MapFrom(e => e.Procedimentos.Select(x => x.ProcedimentoId).ToList()))
            .ForCtorParam("ImagemUrls", o => o.MapFrom(e => ImagemUrls(e)));
        CreateMap<Credenciado, EmpresaPlanoVm>()
            .ForCtorParam("CredenciadoId", o => o.MapFrom(e => e.Id))
            .ForCtorParam("PlanoId", o => o.MapFrom(e => e.Plano!.Id))
            .ForCtorParam("Plano", o => o.MapFrom(e => e.Plano!.Nome))
            .ForCtorParam("Valor", o => o.MapFrom(e => e.Plano!.Valor))
            .ForCtorParam("Periodicidade", o => o.MapFrom(e => e.Plano!.Periodicidade));
        CreateMap<Plano, PlanoVm>();
        CreateMap<PlanoVm, Plano>().ConvertUsing((vm, entity, _) =>
        {
            entity ??= new Plano(vm.Nome, vm.Descricao, vm.Valor, vm.Periodicidade, vm.DataValidade, vm.TipoPessoa);
            entity.Atualizar(vm.Nome, vm.Descricao, vm.Valor, vm.Periodicidade, vm.DataValidade, vm.TipoPessoa);
            return entity;
        });

        CreateMap<Beneficiario, BeneficiarioVm>();
        // The service reconciles children; mapping never replaces tracked relationships.
        CreateMap<BeneficiarioVm, Beneficiario>().ConvertUsing((vm, entity, _) =>
        {
            entity ??= new Beneficiario(vm.Nome, vm.Cpf, vm.DataNascimento, vm.PlanoId, vm.DataInicio, vm.DataValidade, vm.CredenciadoId);
            entity.Atualizar(vm.Nome, vm.Cpf, vm.DataNascimento, vm.Telefone, vm.Email, vm.Endereco,
                vm.PlanoId, vm.DataInicio, vm.DataValidade, (StatusBeneficiario)vm.Status, vm.CredenciadoId);
            entity.DefinirPessoa(vm.TipoPessoa, vm.EmpresaBeneficiadaId);
            return entity;
        });
        CreateMap<Dependente, DependenteVm>();
        CreateMap<DependenteVm, Dependente>().ConvertUsing((vm, entity, _) =>
        {
            if (entity is null) throw new InvalidOperationException("Crie o dependente com o beneficiário vinculado antes de mapear.");
            if (!vm.DataNascimento.HasValue || !vm.GrauParentesco.HasValue)
                throw new ArgumentException("Informe nascimento e parentesco do dependente.");
            entity.Atualizar(vm.Nome, vm.Cpf, vm.DataNascimento.Value, vm.GrauParentesco.Value);
            return entity;
        });

        CreateMap<Credenciado, CredenciadoVm>()
            .ForMember(vm => vm.EspecialidadeIds, o => o.MapFrom(e => e.Especialidades.Select(x => x.EspecialidadeId)))
            .ForMember(vm => vm.ProcedimentoIds, o => o.MapFrom(e => e.Procedimentos.Select(x => x.ProcedimentoId)))
            .ForMember(vm => vm.ImagemUrls, o => o.MapFrom(e => ImagemUrls(e)));
        CreateMap<CredenciadoVm, Credenciado>().ConvertUsing((vm, entity, _) =>
        {
            if (!vm.Tipo.HasValue || !vm.StatusCredenciamento.HasValue
                || ((!vm.PlanoId.HasValue || vm.PlanoId == Guid.Empty)
                    && (!vm.DescontoId.HasValue || vm.DescontoId == Guid.Empty)))
                throw new ArgumentException("Informe tipo, situação e plano ou desconto do credenciado.");
            entity ??= new Credenciado(vm.RazaoSocial, vm.NomeFantasia, vm.Cnpj, vm.Telefone, vm.WhatsApp,
                vm.Email, vm.Cep, vm.Endereco, vm.Cidade, vm.Uf, vm.Observacoes, vm.Tipo.Value, vm.StatusCredenciamento.Value);
            entity.Atualizar(vm.RazaoSocial, vm.NomeFantasia, vm.Cnpj, vm.Telefone, vm.WhatsApp,
                vm.Email, vm.Cep, vm.Endereco, vm.Cidade, vm.Uf, vm.Observacoes, vm.Tipo.Value, vm.StatusCredenciamento.Value);
            if (vm.PlanoId is Guid planoId) entity.DefinirPlano(planoId);
            if (vm.DescontoId is Guid descontoId) entity.DefinirDesconto(descontoId);
            return entity;
        });

        CreateMap<Especialidade, EspecialidadeVm>();
        CreateMap<EspecialidadeVm, Especialidade>().ConvertUsing((vm, entity, _) =>
        {
            entity ??= new Especialidade(vm.Nome);
            entity.Atualizar(vm.Nome, vm.Ativo);
            return entity;
        });
        CreateMap<Procedimento, ProcedimentoVm>();
        CreateMap<ProcedimentoVm, Procedimento>().ConvertUsing((vm, entity, _) =>
        {
            entity ??= new Procedimento(vm.Nome);
            entity.Atualizar(vm.Nome, vm.Ativo);
            return entity;
        });
        CreateMap<CredenciadoEspecialidade, CredenciadoEspecialidadeVm>();
        CreateMap<CredenciadoEspecialidadeVm, CredenciadoEspecialidade>()
            .ConvertUsing(vm => new CredenciadoEspecialidade(vm.CredenciadoId, vm.EspecialidadeId));
        CreateMap<CredenciadoProcedimento, CredenciadoProcedimentoVm>();
        CreateMap<CredenciadoProcedimentoVm, CredenciadoProcedimento>()
            .ConvertUsing(vm => new CredenciadoProcedimento(vm.CredenciadoId, vm.ProcedimentoId));

        CreateMap<Banner, BannerVm>();
        CreateMap<BannerVm, Banner>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.CriadoEm, o => o.Ignore())
            .ForMember(e => e.Imagem, o => o.Ignore())
            .ForMember(e => e.ContentType, o => o.Ignore())
            .ForMember(e => e.Titulo, o => o.MapFrom(vm => vm.Titulo.Trim()));
        CreateMap<ContatoEntradaVm, Contato>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.EnviadoEm, o => o.Ignore())
            .ForMember(e => e.NotificacaoSuspensa, o => o.Ignore());
        CreateMap<Contato, ContatoVm>();
        CreateMap<IntencaoVenda, IntencaoVendaVm>();
        CreateMap<ContatoVm, Contato>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.EnviadoEm, o => o.Ignore())
            .ForMember(e => e.NotificacaoSuspensa, o => o.Ignore());

        CreateMap<Usuario, UsuarioVm>()
            .ForMember(vm => vm.UsuarioId, o => o.MapFrom(e => e.Id))
            .ForMember(vm => vm.Menus, o => o.MapFrom(e => MenusAdministrativos.Ler(e.MenusPermitidos)));
        CreateMap<Usuario, UsuarioEdicaoVm>()
            .ForMember(vm => vm.UsuarioId, o => o.MapFrom(e => e.Id))
            .ForMember(vm => vm.Senha, o => o.Ignore())
            .ForMember(vm => vm.Menus, o => o.MapFrom(e => MenusAdministrativos.Ler(e.MenusPermitidos)));

        CreateMap<UsuarioCadastroVm, Usuario>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.Ativo, o => o.Ignore())
            .ForMember(e => e.SenhaHash, o => o.Ignore())
            .ForMember(e => e.NomeCompleto, o => o.MapFrom(vm => vm.NomeCompleto.Trim()))
            .ForMember(e => e.Email, o => o.MapFrom(vm => vm.Email.Trim()))
            .ForMember(e => e.EmailNormalizado, o => o.MapFrom(vm => vm.Email.Trim().ToUpperInvariant()))
            .ForMember(e => e.MenusPermitidos, o => o.MapFrom(vm => MenusAdministrativos.Gravar(vm.Menus)));
        CreateMap<UsuarioEdicaoVm, Usuario>()
            .ForMember(e => e.Id, o => o.Ignore())
            .ForMember(e => e.Ativo, o => o.Ignore())
            .ForMember(e => e.SenhaHash, o => o.Ignore())
            .ForMember(e => e.NomeCompleto, o => o.MapFrom(vm => vm.NomeCompleto.Trim()))
            .ForMember(e => e.Email, o => o.MapFrom(vm => vm.Email.Trim()))
            .ForMember(e => e.EmailNormalizado, o => o.MapFrom(vm => vm.Email.Trim().ToUpperInvariant()))
            .ForMember(e => e.MenusPermitidos, o => o.MapFrom(vm => MenusAdministrativos.Gravar(vm.Menus)));
    }

    private static List<string> ImagemUrls(Credenciado credenciado) =>
        new[] { credenciado.ImagemUrl }
            .Concat(credenciado.Imagens.OrderBy(x => x.CriadoEm).ThenBy(x => x.Id).Select(x => x.Url))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
}
