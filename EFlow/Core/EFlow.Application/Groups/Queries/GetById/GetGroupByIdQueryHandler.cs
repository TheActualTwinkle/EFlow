using EFlow.Application.Common.Errors;
using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Groups.Queries;

public class GetGroupByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetGroupByIdQuery, Result<GroupDto>>
{
    public async Task<Result<GroupDto>> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var group = await unitOfWork
            .GetRepository<IGroupRepository>()
            .GetByIdAsync(request.Id, cancellationToken);

        if (group is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Group not found")
                    .WithId(request.Id));

        return group.Adapt<GroupDto>();
    }
}