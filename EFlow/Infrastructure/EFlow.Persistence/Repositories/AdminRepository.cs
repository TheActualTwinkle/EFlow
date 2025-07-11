using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;

namespace EFlow.Persistence.Repositories;

public class AdminRepository(ApplicationDbContext context) :
    RepositoryBase<Admin>(context), IAdminRepository
{
    public async Task CreateAsync(Admin admin, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(admin, cancellationToken);

    public IEnumerable<Admin> GetAll() =>
        GetAllInternal();

    public async Task<Admin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await GetByIdInternalAsync(id, cancellationToken);

    public void Delete(Admin admin) =>
        DeleteInternal(admin);
}