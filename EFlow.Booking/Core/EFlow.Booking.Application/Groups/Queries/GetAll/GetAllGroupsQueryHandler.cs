using EFlow.Booking.Contracts.Groups;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Queries;

public class GetAllGroupsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllGroupsQuery, Result<IEnumerable<GroupView>>>
{
    public async Task<Result<IEnumerable<GroupView>>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await unitOfWork
            .GetQueryService<IGroupQueryService>()
            .GetAllAsync(cancellationToken);

        return Result.Ok(groups);
    }
}
