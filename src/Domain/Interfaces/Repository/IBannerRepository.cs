using PVHSAUDE.Domain.Entities;
namespace PVHSAUDE.Domain.Interfaces.Repository;

public interface IBannerRepository : IEntityRepository<Banner>
{
    Task<List<Banner>> ListarMetadadosAsync(bool ativos, CancellationToken ct);
    Task<Banner?> ObterMetadadosAsync(Guid id, CancellationToken ct);
}
