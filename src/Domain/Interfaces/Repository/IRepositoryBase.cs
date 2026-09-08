namespace PVHSAUDE.Domain.Interfaces.Repository
{
    public interface IRepositoryBase<TEntity> : IDisposable where TEntity : class
    {
        Task AddAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task RemoveAsync(TEntity entity);
        Task CommitAsync();
        Task CheduledCommitAsync(Guid sistemaIdSetContextInfo);

    }
}