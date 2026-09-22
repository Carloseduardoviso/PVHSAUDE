using AutoMapper;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class CredenciadoService(IEntityRepository<Credenciado> repository, IEntityRepository<Plano> planos,
    IEntityRepository<CredenciadoEspecialidade> especialidades, IEntityRepository<CredenciadoProcedimento> procedimentos,
    IUnitOfWork work, IMapper mapper, IImagemStorage storage, IEntityRepository<CredenciadoImagem> imagens,
    IEntityRepository<Beneficiario> beneficiarios, IEntityRepository<Desconto>? descontos = null) : ICredenciadoService
{
    public async Task<List<CredenciadoRespostaVm>> ListarAsync(CancellationToken ct) =>
        mapper.Map<List<CredenciadoRespostaVm>>((await repository.ListarAsync(null, ct, x => x.Especialidades, x => x.Procedimentos, x => x.Imagens)).OrderBy(x => x.NomeFantasia));
    private async Task<Credenciado> Encontrar(Guid id, CancellationToken ct) =>
        await repository.ObterAsync(x => x.Id == id, ct, x => x.Especialidades, x => x.Procedimentos, x => x.Imagens) ?? throw new ServiceException(ServiceError.NotFound);
    public async Task<CredenciadoRespostaVm> ObterAsync(Guid id, CancellationToken ct) =>
        mapper.Map<CredenciadoRespostaVm>(await Encontrar(id, ct));
    private async Task Validar(Guid? id, CredenciadoEntradaVm vm, CancellationToken ct)
    {
        if (vm.Tipo == PVHSAUDE.Domain.Enuns.TipoCredenciado.EmpresaBeneficiada)
            throw new ServiceException(ServiceError.Invalid, "Selecione um menu de credenciamento válido.");
        if (vm.DescontoId is Guid descontoId && descontos is not null && !await descontos.ExisteAsync(x => x.Id == descontoId && x.Ativo, ct))
            throw new ServiceException(ServiceError.Invalid, "Selecione um desconto cadastrado.");
        if (vm.DescontoId is null && !await planos.ExisteAsync(x => x.Id == vm.PlanoId, ct))
            throw new ServiceException(ServiceError.Invalid, "Selecione um desconto cadastrado.");
        var cnpj = new string(vm.Cnpj.Where(char.IsDigit).ToArray());
        if (await repository.ExisteAsync(x => x.Id != id && x.Cnpj == cnpj, ct))
            throw new ServiceException(ServiceError.Conflict, "Já existe uma empresa credenciada com este CNPJ.");
    }
    public async Task<CredenciadoRespostaVm> CriarAsync(CredenciadoEntradaVm vm, CancellationToken ct)
    {
        await Validar(null, vm, ct);
        var entity = mapper.Map<Credenciado>(mapper.Map<CredenciadoVm>(vm));
        repository.Adicionar(entity);
        Sincronizar(entity, vm);
        await work.SalvarAsync(ct);
        return mapper.Map<CredenciadoRespostaVm>(entity);
    }
    public async Task AtualizarAsync(Guid id, CredenciadoEntradaVm vm, CancellationToken ct)
    {
        var entity = await Encontrar(id, ct);
        await Validar(id, vm, ct);
        mapper.Map(mapper.Map<CredenciadoVm>(vm), entity);
        Sincronizar(entity, vm);
        await work.SalvarAsync(ct);
    }
    public async Task ExcluirAsync(Guid id, CancellationToken ct)
    {
        var entity = await Encontrar(id, ct);
        if (await beneficiarios.ExisteAsync(x => x.CredenciadoId == id, ct))
            throw new ServiceException(ServiceError.Conflict, "Este credenciamento possui beneficiários vinculados e não pode ser excluído.");

        var urls = entity.Imagens.Select(x => x.Url).Append(entity.ImagemUrl)
            .Where(x => !string.IsNullOrWhiteSpace(x)).Distinct().ToList();
        foreach (var especialidade in entity.Especialidades) especialidades.Remover(especialidade);
        foreach (var procedimento in entity.Procedimentos) procedimentos.Remover(procedimento);
        foreach (var imagem in entity.Imagens) imagens.Remover(imagem);
        repository.Remover(entity);
        await work.SalvarAsync(ct);
        foreach (var url in urls) await storage.ExcluirCredenciadoAsync(url!, ct);
    }
    private void Sincronizar(Credenciado entity, CredenciadoEntradaVm vm)
    {
        foreach (var antigo in entity.Especialidades.Where(x => !vm.EspecialidadeIds.Contains(x.EspecialidadeId)).ToList())
        { especialidades.Remover(antigo); entity.Especialidades.Remove(antigo); }
        foreach (var id in vm.EspecialidadeIds.Distinct().Where(id => !entity.Especialidades.Any(x => x.EspecialidadeId == id)))
        {
            var novo = mapper.Map<CredenciadoEspecialidade>(new CredenciadoEspecialidadeVm(entity.Id, id));
            entity.Especialidades.Add(novo);
            especialidades.Adicionar(novo);
        }
        foreach (var antigo in entity.Procedimentos.Where(x => !vm.ProcedimentoIds.Contains(x.ProcedimentoId)).ToList())
        { procedimentos.Remover(antigo); entity.Procedimentos.Remove(antigo); }
        foreach (var id in vm.ProcedimentoIds.Distinct().Where(id => !entity.Procedimentos.Any(x => x.ProcedimentoId == id)))
        {
            var novo = mapper.Map<CredenciadoProcedimento>(new CredenciadoProcedimentoVm(entity.Id, id));
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
        var url = await storage.SalvarCredenciadoAsync(id, extensao, conteudo, ct);
        entity.DefinirImagem(url);
        entity.AdicionarImagem(url);
        await work.SalvarAsync(ct);
        return url;
    }
    public async Task RemoverImagemAsync(Guid id, string url, CancellationToken ct)
    {
        var entity = await Encontrar(id, ct);
        var caminho = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.AbsolutePath : url;
        var vinculadas = entity.Imagens.Where(x => x.Url == caminho).ToList();
        if (vinculadas.Count == 0 && entity.ImagemUrl != caminho)
            throw new ServiceException(ServiceError.NotFound);

        foreach (var imagem in vinculadas)
        {
            imagens.Remover(imagem);
            entity.Imagens.Remove(imagem);
        }
        if (entity.ImagemUrl == caminho)
            entity.DefinirImagem(entity.Imagens.OrderByDescending(x => x.CriadoEm).ThenByDescending(x => x.Id).FirstOrDefault()?.Url);
        await work.SalvarAsync(ct);
        await storage.ExcluirCredenciadoAsync(caminho, ct);
    }
}
