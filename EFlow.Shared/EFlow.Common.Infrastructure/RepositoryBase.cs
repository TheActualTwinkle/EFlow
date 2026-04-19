using EFlow.Common.Domain;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public abstract class RepositoryBase<TEntity>(DbContext context)
    where TEntity : Entity
{
    protected async Task CreateInternalAsync(TEntity entity, CancellationToken cancellationToken = new()) =>
        await context.Set<TEntity>().AddAsync(entity, cancellationToken);

    protected async Task CreateBulkInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = new()) =>
        await context.Set<TEntity>().AddRangeAsync(entities, cancellationToken);

    protected async Task<IEnumerable<TEntity>> GetAllInternalAsync(CancellationToken cancellationToken = new()) =>
        await context.Set<TEntity>().ToListAsync(cancellationToken);

    protected async Task<TEntity?> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await context.Set<TEntity>().FindAsync([id], cancellationToken);

    protected async Task<bool> ExistsInternalAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken) is not null;

    protected void UpdateInternal(TEntity entity) =>
        context.Set<TEntity>().Update(entity);

    protected void UpdateBulkInternal(IEnumerable<TEntity> entities) =>
        context.Set<TEntity>().UpdateRange(entities);

    protected async Task DeleteInternalAsync(TEntity entity) =>
        context.Set<TEntity>().Remove(entity);

    protected async Task DeleteBulkInternal(IEnumerable<TEntity> entities) =>
        context.Set<TEntity>().RemoveRange(entities);
}