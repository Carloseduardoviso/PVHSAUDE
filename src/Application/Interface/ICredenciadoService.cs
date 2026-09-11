using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface ICredenciadoService
{
    Task<List<CredenciadoRespostaVm>> ListarAsync(CancellationToken ct);
    Task<CredenciadoRespostaVm> ObterAsync(Guid id, CancellationToken ct);
    Task<CredenciadoRespostaVm> CriarAsync(CredenciadoEntradaVm vm, CancellationToken ct);
    Task AtualizarAsync(Guid id, CredenciadoEntradaVm vm, CancellationToken ct);
    Task<string> UploadImagemAsync(Guid id, string nome, long tamanho, Stream conteudo, CancellationToken ct);
}
