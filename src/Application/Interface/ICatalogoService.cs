using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface ICatalogoService
{
    Task<List<EspecialidadeRespostaVm>> EspecialidadesAsync(CancellationToken ct);
    Task<List<ProcedimentoRespostaVm>> ProcedimentosAsync(CancellationToken ct);
    Task<EspecialidadeRespostaVm> CriarEspecialidadeAsync(CatalogoEntradaVm vm, CancellationToken ct);
    Task<ProcedimentoRespostaVm> CriarProcedimentoAsync(CatalogoEntradaVm vm, CancellationToken ct);
    Task<EspecialidadeRespostaVm> AtualizarEspecialidadeAsync(Guid id, CatalogoEntradaVm vm, CancellationToken ct);
    Task<ProcedimentoRespostaVm> AtualizarProcedimentoAsync(Guid id, CatalogoEntradaVm vm, CancellationToken ct);
    Task ExcluirEspecialidadeAsync(Guid id, CancellationToken ct);
    Task ExcluirProcedimentoAsync(Guid id, CancellationToken ct);
}
