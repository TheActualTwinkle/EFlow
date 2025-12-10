using System.Data;
using EFlow.Booking.Domain.Repositories;

namespace EFlow.Booking.Domain;

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