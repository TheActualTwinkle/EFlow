using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.Repositories;

public class GroupRepository(ApplicationDbContext context) :
    IGroupRepository
{
    public async Task CreateAsync(Group group, CancellationToken cancellationToken = new()) =>
        await context.Groups.AddAsync(group, cancellationToken);

    public async Task<IEnumerable<Group>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Groups.ToListAsync(cancellationToken);

    public async Task<Group?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = new()) =>
        await context.Groups.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public void Update(Group group) =>
        context.Groups.Update(group);

    public Task DeleteAsync(Group group)
    {
        context.Groups.Remove(group);
        
        return Task.CompletedTask;
    }
}