namespace PVHSAUDE.Domain.Services
{
    public abstract class ServiceBase<TRepository> : IDisposable where TRepository : class
    {
        protected readonly TRepository Repository;

        protected ServiceBase(TRepository repository)
        {
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        protected async Task ExecuteWithTransactionAsync(Func<Task> operation)
        {
            try
            {
                await operation();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("An error occurred while processing the operation.", ex);
            }
        }

        public virtual void Dispose()
        {
            if (Repository is IDisposable disposableRepository)
            {
                disposableRepository.Dispose();
            }
        }
    }
}
