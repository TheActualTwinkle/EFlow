using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;

namespace EFlow.Persistence.Repositories;

public class GroupRepository(ApplicationDbContext context) :
    RepositoryBase<Group>(context), IGroupRepository
{
    public async Task CreateAsync(Group group, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(group, cancellationToken);

    public IEnumerable<Group> GetAll() =>
        GetAllInternal();

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public void Update(Group group) =>
        UpdateInternal(group);

    public void Delete(Group group) =>
        DeleteInternal(group);
}