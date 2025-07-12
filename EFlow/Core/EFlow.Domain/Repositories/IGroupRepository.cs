using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface IGroupRepository : IRepository
{
    public Task CreateAsync(Group group, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public void Update(Group group);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}