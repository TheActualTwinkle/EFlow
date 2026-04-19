using EFlow.Booking.Domain.Groups;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Groups.Queries;

public class GetAllGroupsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllGroupsQuery, Result<IEnumerable<GroupDto>>>
{
    public async Task<Result<IEnumerable<GroupDto>>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = (await unitOfWork
                .GetRepository<IGroupRepository>()
                .GetAllAsync(cancellationToken))
            .Adapt<IEnumerable<GroupDto>>();

        return Result.Ok(groups);
    }
}