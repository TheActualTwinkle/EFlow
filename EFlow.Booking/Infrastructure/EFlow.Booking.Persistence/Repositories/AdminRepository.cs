using EFlow.Booking.Domain.Admins;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class AdminRepository(ApplicationDbContext context) : IAdminRepository
{
    public async Task CreateAsync(Admin admin, CancellationToken cancellationToken = new()) =>
        await context.Admins.AddAsync(admin, cancellationToken);

    public async Task<IEnumerable<Admin>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Admins.ToListAsync(cancellationToken);

    public async Task<Admin?> GetByIdAsync(AdminId id, CancellationToken cancellationToken = new()) =>
        await context.Admins.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task DeleteAsync(Admin admin) =>
        Task.FromResult(context.Admins.Remove(admin));
}