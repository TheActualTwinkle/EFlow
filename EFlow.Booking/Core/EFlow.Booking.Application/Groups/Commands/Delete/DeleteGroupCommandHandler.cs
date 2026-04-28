using EFlow.Booking.Domain.Groups;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Commands;

public class DeleteGroupCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteGroupCommand, Result>
{
    public async Task<Result> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IGroupRepository>();
        
        var group = await repository.GetByIdAsync(new GroupId(request.Id), cancellationToken);

        if (group is null)
            return Result.Ok();

        group.Delete();

        await repository.DeleteAsync(group);

        return Result.Ok();
    }
}