using System.Data;
using EFlow.Domain.Repositories;

namespace EFlow.Domain;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    public T GetRepository<T>() where T : IRepository;

    public Task BeginTransactionAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = new());

    public Task CommitTransactionAsync(CancellationToken cancellationToken = new());

    public Task RollbackTransactionAsync(CancellationToken cancellationToken = new());

    public Task SaveChangesAsync(CancellationToken cancellationToken = new());
}