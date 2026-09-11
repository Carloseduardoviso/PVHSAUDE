using Infra.Data.Base;
using Microsoft.EntityFrameworkCore;
using PVHSAUDE.Domain.Entities;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Infra.Data;

public class BannerRepository(Context context) : EntityRepository<Banner>(context), IBannerRepository
{
    private IQueryable<Banner> Metadados() => Db.Set<Banner>().AsNoTracking()
        .Select(x => new Banner { Id = x.Id, Titulo = x.Titulo, Ativo = x.Ativo, CriadoEm = x.CriadoEm });
    public Task<List<Banner>> ListarMetadadosAsync(bool ativos, CancellationToken ct) =>
        Metadados().Where(x => !ativos || x.Ativo).OrderByDescending(x => x.CriadoEm).ThenBy(x => x.Id).ToListAsync(ct);
    public Task<Banner?> ObterMetadadosAsync(Guid id, CancellationToken ct) =>
        Metadados().SingleOrDefaultAsync(x => x.Id == id, ct);
}
