using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Contracts.Groups;
using EFlow.Booking.Domain.Groups;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Queries;

public class GetGroupByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetGroupByIdQuery, Result<GroupView>>
{
    public async Task<Result<GroupView>> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await unitOfWork
            .GetQueryService<IGroupQueryService>()
            .GetByIdAsync(new GroupId(request.Id), cancellationToken);

        if (group is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Group not found")
                    .WithId(request.Id));

        return Result.Ok(group);
    }
}
