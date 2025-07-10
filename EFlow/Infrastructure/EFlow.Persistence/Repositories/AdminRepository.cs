using EFlow.Domain.Models;
using EFlow.Persistence.DatabaseContext;
using EFlow.Domain.Repositories;

namespace EFlow.Persistence.Repositories;

public class AdminRepository(ApplicationDbContext context) :
    RepositoryBase<Admin>(context), IAdminRepository
{
    public Task CreateAsync(Admin admin, CancellationToken cancellationToken = new())
    {
        ArgumentNullException.ThrowIfNull(admin);

        return CreateInternalAsync(admin, cancellationToken);
    }

    public Task<Admin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        GetByIdInternalAsync(id, cancellationToken);
}