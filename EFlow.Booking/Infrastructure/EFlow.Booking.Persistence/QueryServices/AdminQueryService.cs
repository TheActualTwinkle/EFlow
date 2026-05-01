using EFlow.Booking.Contracts.Admins;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class AdminQueryService(ApplicationDbContext context) : IAdminQueryService
{
    public async Task<IEnumerable<AdminView>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Admins
            .Select(a => new AdminView
            {
                Id = a.Id.Value,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);
}