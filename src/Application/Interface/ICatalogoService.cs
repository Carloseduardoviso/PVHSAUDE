using PVHSAUDE.Application.ViewModels;
namespace PVHSAUDE.Application.Interface;
public interface ICatalogoService
{
    Task<List<EspecialidadeRespostaVm>> EspecialidadesAsync(CancellationToken ct);
    Task<List<ProcedimentoRespostaVm>> ProcedimentosAsync(CancellationToken ct);
    Task<EspecialidadeRespostaVm> CriarEspecialidadeAsync(CatalogoEntradaVm vm, CancellationToken ct);
    Task<ProcedimentoRespostaVm> CriarProcedimentoAsync(CatalogoEntradaVm vm, CancellationToken ct);
}
