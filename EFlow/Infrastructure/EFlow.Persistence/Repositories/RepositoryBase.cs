using EFlow.Persistence.DatabaseContext;

namespace EFlow.Persistence.Repositories;

public abstract class RepositoryBase<TEntity>(ApplicationDbContext context)
    where TEntity : class
{
    protected readonly ApplicationDbContext Context = context;

    protected async Task CreateInternalAsync(TEntity entity, CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>().AddAsync(entity, cancellationToken);

    protected async Task CreateBulkInternalAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>().AddRangeAsync(entities, cancellationToken);

    protected IEnumerable<TEntity> GetAllInternal() =>
        Context.SetEntity<TEntity>().AsEnumerable();

    protected async Task<TEntity?> GetByIdInternalAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await Context.SetEntity<TEntity>().FindAsync([id], cancellationToken);

    protected async Task<bool> ExistsInternalAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken) is not null;

    protected void UpdateInternal(TEntity entity) =>
        Context.SetEntity<TEntity>().Update(entity);

    protected void UpdateBulkInternal(IEnumerable<TEntity> entities) =>
        Context.SetEntity<TEntity>().UpdateRange(entities);

    protected void DeleteInternal(TEntity entity) =>
        Context.SetEntity<TEntity>().Remove(entity);

    protected void DeleteBulkInternal(IEnumerable<TEntity> entities) =>
        Context.SetEntity<TEntity>().RemoveRange(entities);
}