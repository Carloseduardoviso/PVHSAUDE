using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface IEmpresaBeneficiadaService
{
    Task<List<EmpresaBeneficiadaRespostaVm>> ListarAsync(CancellationToken ct);
    Task<EmpresaBeneficiadaRespostaVm> ObterAsync(Guid id, CancellationToken ct);
    Task<EmpresaBeneficiadaRespostaVm> CriarAsync(EmpresaBeneficiadaEntradaVm vm, CancellationToken ct);
    Task AtualizarAsync(Guid id, EmpresaBeneficiadaEntradaVm vm, CancellationToken ct);
    Task<string> UploadImagemAsync(Guid id, string nome, long tamanho, Stream conteudo, CancellationToken ct);
}
