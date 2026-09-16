using AutoMapper;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;

namespace PVHSAUDE.Application.AppService;

public class IntencaoVendaService(IEntityRepository<IntencaoVenda> repository, IEntityRepository<Plano> planos, IUnitOfWork work, IMapper mapper) : IIntencaoVendaService
{
    public async Task<List<IntencaoVendaVm>> ListarAsync(CancellationToken ct) => mapper.Map<List<IntencaoVendaVm>>((await repository.ListarAsync(null, ct)).OrderByDescending(x => x.CriadoEm));

    public async Task CriarAsync(IntencaoVendaEntradaVm vm, CancellationToken ct)
    {
        var plano = await planos.ObterAsync(x => x.Id == vm.PlanoId && x.TipoPessoa == Domain.Enuns.TipoPessoa.Fisica, ct)
            ?? throw new ServiceException(ServiceError.NotFound);
        var entity = new IntencaoVenda(vm.Nome, vm.Email, vm.Telefone, vm.Cpf, plano.Id, plano.Nome, plano.Valor,
            vm.QuantidadeDependentes, vm.Endereco, vm.Dependentes);
        repository.Adicionar(entity);
        await work.SalvarAsync(ct);
    }

    public async Task AtualizarStatusAsync(Guid id, Domain.Enuns.StatusIntencaoVenda status, CancellationToken ct)
    {
        var entity = await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound);
        entity.AtualizarStatus(status);
        await work.SalvarAsync(ct);
    }
}
