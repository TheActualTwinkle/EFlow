using System.Data;
using EFlow.Common.Domain;

namespace EFlow.Common.Infrastructure;

public interface IUnitOfWork : IAsyncDisposable
{
    public T GetRepository<T>() where T : IRepository;

    public Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = new());

    public Task CommitTransactionAsync(CancellationToken cancellationToken = new());

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = new());
}