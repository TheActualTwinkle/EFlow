using System.Data;

namespace EFlow.Booking.Domain;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> CreateTransactionalAsync(
        IsolationLevel isolationLevel = IsolationLevel.ReadCommitted,
        CancellationToken cancellationToken = new());

    Task<IUnitOfWork> CreateNonTransactionalAsync(CancellationToken cancellationToken = new());
}