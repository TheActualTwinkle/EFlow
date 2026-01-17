using EFlow.Common.Domain.Models;
using EFlow.Common.Domain;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class AdminRepository(ApplicationDbContext context) :
    RepositoryBase<Admin>(context), IAdminRepository
{
    public async Task CreateAsync(Admin admin, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(admin, cancellationToken);

    public async Task<IEnumerable<Admin>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Admins
            .Include(a => a.Identity)
            .ToListAsync(cancellationToken);

    public async Task<Admin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await context.Admins
            .Include(a => a.Identity)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}