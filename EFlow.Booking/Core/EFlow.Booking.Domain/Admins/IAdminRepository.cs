using EFlow.Common.Domain;

namespace EFlow.Booking.Domain.Admins;

public interface IAdminRepository : IRepository
{
    public Task CreateAsync(Admin admin, CancellationToken cancellationToken = new());

    public Task<IEnumerable<Admin>> GetAllAsync(CancellationToken cancellationToken = new());

    public Task<Admin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new());
}