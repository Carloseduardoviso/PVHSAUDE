using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IUsuarioService
{
    Task<List<UsuarioVm>> ListarAsync(CancellationToken ct);
    Task<UsuarioVm> ObterAsync(Guid id, CancellationToken ct);
    Task<UsuarioVm> CriarAsync(UsuarioCadastroVm vm, CancellationToken ct);
    Task EditarAsync(Guid id, UsuarioEdicaoVm vm, CancellationToken ct);
    Task InativarAsync(Guid id, CancellationToken ct);
    Task AtivarAsync(Guid id, CancellationToken ct);
    Task ExcluirAsync(Guid id, CancellationToken ct);
}
