using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;

namespace EFlow.Persistence.Repositories;

public class GroupRepository(ApplicationDbContext context) :
    RepositoryBase<Group>(context), IGroupRepository
{
    public async Task CreateAsync(Group group, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(group, cancellationToken);

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await GetAllInternalAsync(cancellationToken);

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public void Update(Group group) =>
        UpdateInternal(group);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}