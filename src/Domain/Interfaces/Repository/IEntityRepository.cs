using System.Linq.Expressions;
namespace PVHSAUDE.Domain.Interfaces.Repository;

public interface IEntityRepository<T> where T : class
{
    Task<List<T>> ListarAsync(Expression<Func<T, bool>>? filtro, CancellationToken ct, params Expression<Func<T, object>>[] includes);
    Task<T?> ObterAsync(Expression<Func<T, bool>> filtro, CancellationToken ct, params Expression<Func<T, object>>[] includes);
    Task<bool> ExisteAsync(Expression<Func<T, bool>> filtro, CancellationToken ct);
    void Adicionar(T entidade);
    void Remover(T entidade);
}
public interface IUnitOfWork
{
    Task SalvarAsync(CancellationToken ct);
    Task<IRepositoryTransaction> BloquearUsuariosAsync(CancellationToken ct);
}
public interface IRepositoryTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}
public class RegistroDuplicadoException : Exception
{
    public RegistroDuplicadoException(Exception inner) : base("Registro duplicado.", inner) { }
}
