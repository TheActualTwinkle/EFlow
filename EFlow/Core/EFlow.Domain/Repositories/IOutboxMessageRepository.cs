using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface IOutboxMessageRepository : IRepository
{
    public Task CreateAsync(OutboxMessage message, CancellationToken cancellationToken = new());

    public Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = new());

    public Task MarkAsProcessedAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = new());
    
    public Task AddErrorAsync(Guid id, string error, CancellationToken cancellationToken = new());

    public Task DeleteProcessedAsync(DateTime beforeDate, CancellationToken cancellationToken = new());
}