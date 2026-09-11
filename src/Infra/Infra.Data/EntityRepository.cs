using System.Linq.Expressions;
using Infra.Data.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using PVHSAUDE.Domain.Interfaces.Repository;
namespace PVHSAUDE.Infra.Data;

public class EntityRepository<T>(Context context) : IEntityRepository<T> where T : class
{
    protected Context Db { get; } = context;
    private IQueryable<T> Query(params Expression<Func<T, object>>[] includes)
    {
        IQueryable<T> query = Db.Set<T>();
        foreach (var include in includes) query = query.Include(include);
        return query;
    }
    public Task<List<T>> ListarAsync(Expression<Func<T, bool>>? filtro, CancellationToken ct, params Expression<Func<T, object>>[] includes)
    {
        var query = Query(includes).AsNoTracking();
        return (filtro is null ? query : query.Where(filtro)).ToListAsync(ct);
    }
    public Task<T?> ObterAsync(Expression<Func<T, bool>> filtro, CancellationToken ct, params Expression<Func<T, object>>[] includes)
        => Query(includes).SingleOrDefaultAsync(filtro, ct);
    public Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro, CancellationToken ct)
        => Db.Set<T>().AnyAsync(filtro, ct);
    public void Adicionar(T entidade) => Db.Set<T>().Add(entidade);
    public void Remover(T entidade) => Db.Set<T>().Remove(entidade);
}
public class UnitOfWork(Context context) : IUnitOfWork
{
    public async Task SalvarAsync(CancellationToken ct)
    {
        try { await context.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        { throw new RegistroDuplicadoException(ex); }
    }
    public async Task<IRepositoryTransaction> BloquearUsuariosAsync(CancellationToken ct)
    {
        var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            await context.Database.ExecuteSqlRawAsync(
                "DECLARE @r int; EXEC @r = sp_getapplock @Resource = 'UsuariosAdministracao', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 10000; IF @r < 0 THROW 50001, 'Não foi possível bloquear o cadastro de usuários.', 1;", ct);
            return new RepositoryTransaction(transaction);
        }
        catch { await transaction.DisposeAsync(); throw; }
    }
    private sealed class RepositoryTransaction(IDbContextTransaction transaction) : IRepositoryTransaction
    {
        public Task CommitAsync(CancellationToken ct) => transaction.CommitAsync(ct);
        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
