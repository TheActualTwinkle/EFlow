using EFlow.Domain.Models;

namespace EFlow.Domain.Repositories;

public interface IAdminRepository : IRepository
{
    public Task CreateAsync(Admin admin, CancellationToken cancellationToken = new());

    public IEnumerable<Admin> GetAll();

    public Task<Admin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new());

    public void Delete(Admin admin);
}