using EFlow.Common.Domain;

namespace EFlow.Booking.Contracts.Admins;

public interface IAdminQueryService : IQueryService
{
    public Task<IEnumerable<AdminView>> GetAllAsync(CancellationToken cancellationToken = new());
}