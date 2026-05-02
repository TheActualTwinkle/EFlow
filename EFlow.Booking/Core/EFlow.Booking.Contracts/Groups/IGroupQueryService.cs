using EFlow.Booking.Domain.Groups;
using EFlow.Common.Domain;

namespace EFlow.Booking.Contracts.Groups;

public interface IGroupQueryService : IQueryService
{
    public Task<IEnumerable<GroupView>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<GroupView?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = new());
    
    public Task<IEnumerable<GroupView>> GetByIdsAsync(IEnumerable<GroupId> ids, CancellationToken cancellationToken = new());
}