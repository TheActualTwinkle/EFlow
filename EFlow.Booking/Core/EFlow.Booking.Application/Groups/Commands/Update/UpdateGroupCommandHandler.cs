using EFlow.Booking.Application.Common.Errors;
using EFlow.Booking.Application.Common.Errors.Abstractions;
using EFlow.Booking.Domain.Groups;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Commands.Update;

public class UpdateGroupCommandHandler(IUnitOfWork unitOfWork, ISystemClock systemClock)
    : IRequestHandler<UpdateGroupCommand, Result>
{
    public async Task<Result> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IGroupRepository>();

        var group = await repository.GetByIdAsync(new GroupId(request.Id), cancellationToken);

        if (group is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Group not found")
                    .WithId(request.Id));

        var existingGroupNames = (await repository.GetAllAsync(cancellationToken))
            .Where(existingGroup => existingGroup.Id != group.Id)
            .Select(existingGroup => existingGroup.GetName());

        group.Update(request.Patch, systemClock.UtcNow, existingGroupNames);

        repository.Update(group);

        return Result.Ok();
    }
}
