using EFlow.Booking.Domain;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public abstract class RepositoryBase<TEntity>(ApplicationDbContext context)
    where TEntity : class, IEntity
{
    protected readonly ApplicationDbContext Context = context;

    protected async Task CreateInternalAsync(TEntity entity, CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>().AddAsync(entity, cancellationToken);

    protected async Task CreateBulkInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>().AddRangeAsync(entities, cancellationToken);

    protected async Task<IEnumerable<TEntity>> GetAllInternalAsync(CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>().ToListAsync(cancellationToken);

    protected async Task<TEntity?> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>().FindAsync([id], cancellationToken);

    protected async Task<bool> ExistsInternalAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken) is not null;

    protected void UpdateInternal(TEntity entity) =>
        Context.SetEntity<TEntity>().Update(entity);

    protected void UpdateBulkInternal(IEnumerable<TEntity> entities) =>
        Context.SetEntity<TEntity>().UpdateRange(entities);

    protected async Task DeleteInternalAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>()
            .Where(e => e.Id == id)
            .ExecuteDeleteAsync(cancellationToken);

    protected async Task DeleteBulkInternal(IEnumerable<Guid> ids, CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>()
            .Where(e => ids.Contains(e.Id))
            .ExecuteDeleteAsync(cancellationToken);
}