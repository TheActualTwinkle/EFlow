using EFlow.Booking.Domain.Models;
using EFlow.Booking.Domain;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class GroupRepository(ApplicationDbContext context) :
    RepositoryBase<Group>(context), IGroupRepository
{
    public async Task CreateAsync(Group group, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(group, cancellationToken);

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Groups
            .Include(s => s.Students)
            .ToListAsync(cancellationToken);

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await context.Groups
            .Include(s => s.Students)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public void Update(Group group) =>
        UpdateInternal(group);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = new()) =>
        DeleteInternalAsync(id, cancellationToken);
}