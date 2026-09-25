using AutoMapper;
using Microsoft.AspNetCore.Identity;
using PVHSAUDE.Application.ViewModels;
using PVHSAUDE.Application.Interface;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Enuns;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Application.AppService;

public class UsuarioService(IEntityRepository<Usuario> repository, IUnitOfWork work, IMapper mapper, IPasswordHasher<Usuario> hasher) : IUsuarioService
{
    public async Task<List<UsuarioVm>> ListarAsync(CancellationToken ct) =>
        mapper.Map<List<UsuarioVm>>((await repository.ListarAsync(x => x.Role != Role.Beneficiario, ct)).OrderBy(x => x.NomeCompleto));
    public async Task<UsuarioVm> ObterAsync(Guid id, CancellationToken ct) =>
        mapper.Map<UsuarioVm>(await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound));
    private async Task ValidarEmail(Guid? id, string email, CancellationToken ct)
    {
        var normalizado = email.Trim().ToUpperInvariant();
        if (await repository.ExisteAsync(x => x.Id != id && x.EmailNormalizado == normalizado, ct))
            throw Duplicado();
    }
    private static ServiceException Duplicado() => new(ServiceError.Conflict, "Já existe um usuário com este e-mail.", true);
    private async Task Salvar(CancellationToken ct)
    {
        try { await work.SalvarAsync(ct); }
        catch (RegistroDuplicadoException) { throw Duplicado(); }
    }
    public async Task<UsuarioVm> CriarAsync(UsuarioCadastroVm vm, CancellationToken ct)
    {
        await ValidarEmail(null, vm.Email, ct);
        if (vm.Role == Role.Beneficiario)
            throw new ServiceException(ServiceError.Validation, "Use o cadastro da área do beneficiário.");
        var entity = mapper.Map<Usuario>(vm);
        entity.SenhaHash = hasher.HashPassword(entity, vm.Senha);
        repository.Adicionar(entity);
        await Salvar(ct);
        return mapper.Map<UsuarioVm>(entity);
    }
    public Task EditarAsync(Guid id, UsuarioEdicaoVm vm, CancellationToken ct) => Alterar(id, vm, null, false, ct);
    public Task InativarAsync(Guid id, CancellationToken ct) => Alterar(id, null, false, false, ct);
    public Task AtivarAsync(Guid id, CancellationToken ct) => Alterar(id, null, true, false, ct);
    public Task ExcluirAsync(Guid id, CancellationToken ct) => Alterar(id, null, null, true, ct);
    private async Task Alterar(Guid id, UsuarioEdicaoVm? vm, bool? ativo, bool excluir, CancellationToken ct)
    {
        await using var transaction = await work.BloquearUsuariosAsync(ct);
        var entity = await repository.ObterAsync(x => x.Id == id, ct) ?? throw new ServiceException(ServiceError.NotFound);
        if (entity.Role == Role.Beneficiario)
            throw new ServiceException(ServiceError.Validation, "Use a gestão de acessos dos beneficiários.");
        if (vm?.Role == Role.Beneficiario)
            throw new ServiceException(ServiceError.Validation, "Use o cadastro da área do beneficiário.");
        if (entity.Ativo && entity.Role == Role.Administrador &&
            (excluir || ativo == false || (vm is not null && vm.Role != Role.Administrador)) &&
            !await repository.ExisteAsync(x => x.Id != id && x.Ativo && x.Role == Role.Administrador, ct))
            throw new ServiceException(ServiceError.Conflict, "Não é possível remover o último administrador ativo. Cadastre outro administrador antes.", true);
        if (excluir) repository.Remover(entity);
        else
        {
            if (ativo.HasValue) entity.Ativo = ativo.Value;
            if (vm is not null)
            {
                await ValidarEmail(id, vm.Email, ct);
                mapper.Map(vm, entity);
                if (!string.IsNullOrEmpty(vm.Senha)) entity.SenhaHash = hasher.HashPassword(entity, vm.Senha);
            }
        }
        await Salvar(ct);
        await transaction.CommitAsync(ct);
    }
}
