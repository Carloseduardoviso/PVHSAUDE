using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Application.AutoMapper;
public class EmpresaBeneficiadaProfile : Profile
{
    public EmpresaBeneficiadaProfile()
    {
        CreateMap<EmpresaBeneficiadaEntradaVm, EmpresaBeneficiadaVm>()
            .ForMember(x => x.Id, o => o.Ignore()).ForMember(x => x.ImagemUrl, o => o.Ignore());
        CreateMap<EmpresaBeneficiada, EmpresaBeneficiadaRespostaVm>()
            .ForCtorParam("EspecialidadeIds", o => o.MapFrom(e => e.Especialidades.Select(x => x.EspecialidadeId).ToList()))
            .ForCtorParam("ProcedimentoIds", o => o.MapFrom(e => e.Procedimentos.Select(x => x.ProcedimentoId).ToList()));
        CreateMap<EmpresaBeneficiada, EmpresaBeneficiadaVm>()
            .ForMember(vm => vm.EspecialidadeIds, o => o.MapFrom(e => e.Especialidades.Select(x => x.EspecialidadeId)))
            .ForMember(vm => vm.ProcedimentoIds, o => o.MapFrom(e => e.Procedimentos.Select(x => x.ProcedimentoId)));
        CreateMap<EmpresaBeneficiadaVm, EmpresaBeneficiada>().ConvertUsing((vm, entity, _) =>
        {
            if (!vm.Tipo.HasValue || !vm.StatusCredenciamento.HasValue || !vm.PlanoId.HasValue || vm.PlanoId == Guid.Empty)
                throw new ArgumentException("Informe tipo, situação e plano da empresa beneficiada.");
            entity ??= new EmpresaBeneficiada(vm.RazaoSocial, vm.NomeFantasia, vm.Cnpj, vm.Telefone, vm.WhatsApp,
                vm.Email, vm.Cep, vm.Endereco, vm.Cidade, vm.Uf, vm.Observacoes, vm.Tipo.Value, vm.StatusCredenciamento.Value);
            entity.Atualizar(vm.RazaoSocial, vm.NomeFantasia, vm.Cnpj, vm.Telefone, vm.WhatsApp,
                vm.Email, vm.Cep, vm.Endereco, vm.Cidade, vm.Uf, vm.Observacoes, vm.Tipo.Value, vm.StatusCredenciamento.Value);
            entity.DefinirPlano(vm.PlanoId.Value);
            return entity;
        });
        CreateMap<EmpresaBeneficiadaEspecialidade, EmpresaBeneficiadaEspecialidadeVm>();
        CreateMap<EmpresaBeneficiadaEspecialidadeVm, EmpresaBeneficiadaEspecialidade>()
            .ConvertUsing(vm => new EmpresaBeneficiadaEspecialidade(vm.EmpresaBeneficiadaId, vm.EspecialidadeId));
        CreateMap<EmpresaBeneficiadaProcedimento, EmpresaBeneficiadaProcedimentoVm>();
        CreateMap<EmpresaBeneficiadaProcedimentoVm, EmpresaBeneficiadaProcedimento>()
            .ConvertUsing(vm => new EmpresaBeneficiadaProcedimento(vm.EmpresaBeneficiadaId, vm.ProcedimentoId));
    }
}
