using EFlow.Booking.Domain.Groups;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.Groups.Commands;

public class CreateGroupCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateGroupCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IGroupRepository>();

        var existingGroupNames = (await repository.GetAllAsync(cancellationToken)).Adapt<IEnumerable<GroupDto>>().Select(g => g.Name);
        
        var group = Group.Create(request.Name, existingGroupNames);

        await repository.CreateAsync(group, cancellationToken);

        return Result.Ok(group.Id.Value);
    }
}