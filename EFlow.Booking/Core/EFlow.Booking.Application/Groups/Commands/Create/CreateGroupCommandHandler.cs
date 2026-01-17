using EFlow.Common.Domain.Models;
using EFlow.Common.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Commands;

public class CreateGroupCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateGroupCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = request.Name
        };

        await unitOfWork
            .GetRepository<IGroupRepository>()
            .CreateAsync(group, cancellationToken);

        return Result.Ok(group.Id);
    }
}