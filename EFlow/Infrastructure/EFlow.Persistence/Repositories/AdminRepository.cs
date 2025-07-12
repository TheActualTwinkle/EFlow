using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using EFlow.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Persistence.Repositories;

public class AdminRepository(ApplicationDbContext context) :
    RepositoryBase<Admin>(context), IAdminRepository
{
    public async Task CreateAsync(Admin admin, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(admin, cancellationToken);

    public async Task<IEnumerable<Admin>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await Context.Admins
            .Include(a => a.Identity)
            .ToListAsync(cancellationToken);

    public async Task<Admin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await Context.Admins
            .Where(a => a.Id == id)
            .Include(a => a.Identity)
            .FirstOrDefaultAsync(cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}