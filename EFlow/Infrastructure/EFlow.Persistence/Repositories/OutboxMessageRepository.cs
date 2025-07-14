using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Persistence.Repositories;

public class OutboxMessageRepository(ApplicationDbContext context) :
    RepositoryBase<OutboxMessage>(context), IOutboxMessageRepository
{
    public async Task CreateAsync(OutboxMessage message, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(message, cancellationToken);

    public async Task<IReadOnlyList<OutboxMessage>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = new()) =>
        await Context.OutboxMessages
            .FromSqlRaw(
                """
                    SELECT * FROM "outbox_messages"
                    WHERE "ProcessedAt" IS NULL
                    ORDER BY "CreatedAt"
                    LIMIT {0}
                    FOR UPDATE SKIP LOCKED
                """,
                batchSize)
            .ToListAsync(cancellationToken);

    public async Task MarkAsProcessedAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken = new()) =>
        await Context.OutboxMessages
            .Where(m => ids.Contains(m.Id))
            .ExecuteUpdateAsync(
                u => u.SetProperty(m => m.ProcessedAt, DateTime.UtcNow),
                cancellationToken);

    public async Task DeleteProcessedAsync(DateTime beforeDate, CancellationToken cancellationToken = new()) =>
        await Context.OutboxMessages
            .Where(m => m.ProcessedAt < beforeDate)
            .ExecuteDeleteAsync(cancellationToken);
}