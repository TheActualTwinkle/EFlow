using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface IAdminRepository : IRepository
{
    public Task CreateAsync(Admin admin, CancellationToken cancellationToken = new());
    
    public Task<Admin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());
}