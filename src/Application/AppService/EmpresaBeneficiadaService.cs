using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class EmpresaBeneficiadaService(IEntityRepository<EmpresaBeneficiada> repository, IEntityRepository<Plano> planos,
    IEntityRepository<EmpresaBeneficiadaEspecialidade> especialidades, IEntityRepository<EmpresaBeneficiadaProcedimento> procedimentos,
    IUnitOfWork work, IMapper mapper, IImagemStorage storage, IEntityRepository<Beneficiario> beneficiarios) : IEmpresaBeneficiadaService
{
    public async Task<List<EmpresaBeneficiadaRespostaVm>> ListarAsync(CancellationToken ct) =>
        mapper.Map<List<EmpresaBeneficiadaRespostaVm>>((await repository.ListarAsync(null, ct, x => x.Especialidades, x => x.Procedimentos)).OrderBy(x => x.NomeFantasia));
    private async Task<EmpresaBeneficiada> Encontrar(Guid id, CancellationToken ct) =>
        await repository.ObterAsync(x => x.Id == id, ct, x => x.Especialidades, x => x.Procedimentos) ?? throw new ServiceException(ServiceError.NotFound);
    public async Task<EmpresaBeneficiadaRespostaVm> ObterAsync(Guid id, CancellationToken ct) =>
        mapper.Map<EmpresaBeneficiadaRespostaVm>(await Encontrar(id, ct));
    private async Task Validar(Guid? id, EmpresaBeneficiadaEntradaVm vm, CancellationToken ct)
    {
        if (!await planos.ExisteAsync(x => x.Id == vm.PlanoId, ct))
            throw new ServiceException(ServiceError.Invalid, "Selecione um plano cadastrado.");
        var cnpj = new string(vm.Cnpj.Where(char.IsDigit).ToArray());
        if (await repository.ExisteAsync(x => x.Id != id && x.Cnpj == cnpj, ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe uma empresa beneficiada com este CNPJ.");
    }
    public async Task<EmpresaBeneficiadaRespostaVm> CriarAsync(EmpresaBeneficiadaEntradaVm vm, CancellationToken ct)
    {
        vm.Tipo = PVHSAUDE.Domain.Enuns.TipoCredenciado.EmpresaBeneficiada;
        await Validar(null, vm, ct);
        var entity = mapper.Map<EmpresaBeneficiada>(mapper.Map<EmpresaBeneficiadaVm>(vm));
        repository.Adicionar(entity);
        Sincronizar(entity, vm);
        await work.SalvarAsync(ct);
        return mapper.Map<EmpresaBeneficiadaRespostaVm>(entity);
    }
    public async Task AtualizarAsync(Guid id, EmpresaBeneficiadaEntradaVm vm, CancellationToken ct)
    {
        var entity = await Encontrar(id, ct);
        vm.Tipo = PVHSAUDE.Domain.Enuns.TipoCredenciado.EmpresaBeneficiada;
        await Validar(id, vm, ct);
        mapper.Map(mapper.Map<EmpresaBeneficiadaVm>(vm), entity);
        Sincronizar(entity, vm);
        await work.SalvarAsync(ct);
    }
    public async Task ExcluirAsync(Guid id, CancellationToken ct)
    {
        var entity = await Encontrar(id, ct);
        if (await beneficiarios.ExisteAsync(x => x.EmpresaBeneficiadaId == id, ct))
            throw new ServiceException(ServiceError.Conflict, "Esta empresa possui beneficiários vinculados e não pode ser excluída.");

        foreach (var especialidade in entity.Especialidades) especialidades.Remover(especialidade);
        foreach (var procedimento in entity.Procedimentos) procedimentos.Remover(procedimento);
        repository.Remover(entity);
        await work.SalvarAsync(ct);
        if (!string.IsNullOrWhiteSpace(entity.ImagemUrl))
            await storage.ExcluirCredenciadoAsync(entity.ImagemUrl, ct);
    }
    private void Sincronizar(EmpresaBeneficiada entity, EmpresaBeneficiadaEntradaVm vm)
    {
        foreach (var antigo in entity.Especialidades.Where(x => !vm.EspecialidadeIds.Contains(x.EspecialidadeId)).ToList())
        { especialidades.Remover(antigo); entity.Especialidades.Remove(antigo); }
        foreach (var id in vm.EspecialidadeIds.Distinct().Where(id => !entity.Especialidades.Any(x => x.EspecialidadeId == id)))
        {
            var novo = mapper.Map<EmpresaBeneficiadaEspecialidade>(new EmpresaBeneficiadaEspecialidadeVm(entity.Id, id));
            entity.Especialidades.Add(novo);
            especialidades.Adicionar(novo);
        }
        foreach (var antigo in entity.Procedimentos.Where(x => !vm.ProcedimentoIds.Contains(x.ProcedimentoId)).ToList())
        { procedimentos.Remover(antigo); entity.Procedimentos.Remove(antigo); }
        foreach (var id in vm.ProcedimentoIds.Distinct().Where(id => !entity.Procedimentos.Any(x => x.ProcedimentoId == id)))
        {
            var novo = mapper.Map<EmpresaBeneficiadaProcedimento>(new EmpresaBeneficiadaProcedimentoVm(entity.Id, id));
            entity.Procedimentos.Add(novo);
            procedimentos.Adicionar(novo);
        }
    }
    public async Task<string> UploadImagemAsync(Guid id, string nome, long tamanho, Stream conteudo, CancellationToken ct)
    {
        var entity = await Encontrar(id, ct);
        var extensao = Path.GetExtension(nome).ToLowerInvariant();
        if (tamanho == 0 || tamanho > 5_242_880 || !new[] { ".jpg", ".jpeg", ".png", ".webp" }.Contains(extensao))
            throw new ServiceException(ServiceError.Invalid, "Envie uma imagem JPG, PNG ou WEBP de até 5 MB.");
        entity.DefinirImagem(await storage.SalvarCredenciadoAsync(id, extensao, conteudo, ct));
        await work.SalvarAsync(ct);
        return entity.ImagemUrl!;
    }
}
