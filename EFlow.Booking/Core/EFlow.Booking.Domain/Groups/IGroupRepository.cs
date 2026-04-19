using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Groups;

public interface IGroupRepository : IRepository
{
    public Task CreateAsync(Group group, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public void Update(Group group);

    public Task DeleteAsync(Group group);
}