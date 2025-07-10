using System.Data;
using EFlow.Domain.Repositories;

namespace EFlow.Domain;

public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    public T GetRepository<T>() where T : IRepository;
    
    public Task BeginAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = new());

    public Task CommitAsync(CancellationToken cancellationToken = new());

    public Task RollbackAsync(CancellationToken cancellationToken = new());

    public Task SaveChangesAsync(CancellationToken cancellationToken = new());
}