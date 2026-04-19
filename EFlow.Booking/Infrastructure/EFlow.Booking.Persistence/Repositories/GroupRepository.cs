using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class GroupRepository(ApplicationDbContext context) :
    RepositoryBase<Group>(context), IGroupRepository
{
    public async Task CreateAsync(Group group, CancellationToken cancellationToken = new()) =>
        await CreateInternalAsync(group, cancellationToken);

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Groups.ToListAsync(cancellationToken);

    public async Task<Group?> GetByIdAsync(Guid id, CancellationToken cancellationToken = new()) =>
        await context.Groups.FirstOrDefaultAsync(s => s.Id.Value == id, cancellationToken);

    public void Update(Group group) =>
        UpdateInternal(group);

    public Task DeleteAsync(Group group) =>
        DeleteInternalAsync(group);
}