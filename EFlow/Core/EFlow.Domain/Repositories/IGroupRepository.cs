using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface IGroupRepository : IRepository
{
    public Task CreateAsync(Group group, CancellationToken cancellationToken = new());

    public IEnumerable<Group> GetAll();

    public Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public void Update(Group group);

    public void Delete(Group group);
}