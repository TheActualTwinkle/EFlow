using System.Linq.Expressions;
using EFlow.Booking.Contracts.Groups;
using EFlow.Booking.Domain.Groups;
using EFlow.Booking.Persistence.DatabaseContext;
using Microsoft.EntityFrameworkCore;

namespace EFlow.Booking.Persistence.QueryServices;

public sealed class GroupQueryService(ApplicationDbContext context) : IGroupQueryService
{
    public async Task<IEnumerable<GroupView>> GetAllAsync(CancellationToken cancellationToken = new()) =>
        await context.Groups
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    public async Task<GroupView?> GetByIdAsync(GroupId id, CancellationToken cancellationToken = new()) =>
        await context.Groups
            .Where(g => g.Id == id)
            .Select(MapToView())
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<GroupView>> GetByIdsAsync(IEnumerable<GroupId> ids, CancellationToken cancellationToken = new()) =>
        await context.Groups
            .Where(g => ids.Contains(g.Id))
            .Select(MapToView())
            .ToListAsync(cancellationToken);

    private static Expression<Func<Group, GroupView>> MapToView() =>
        group => new GroupView
        {
            Id = group.Id.Value,
            Name = group.Name
        };
}
