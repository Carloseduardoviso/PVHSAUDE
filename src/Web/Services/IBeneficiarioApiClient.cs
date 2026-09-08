using Web.Models;

namespace Web.Services;

public interface IBeneficiarioApiClient
{
    Task<IReadOnlyCollection<BeneficiarioViewModel>> ListarAsync(CancellationToken cancellationToken = default);
    Task<BeneficiarioViewModel?> ObterAsync(int id, CancellationToken cancellationToken = default);
    Task CriarAsync(BeneficiarioViewModel model, CancellationToken cancellationToken = default);
    Task AtualizarAsync(BeneficiarioViewModel model, CancellationToken cancellationToken = default);
    Task ExcluirAsync(int id, CancellationToken cancellationToken = default);
}
