using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Application.Groups.Commands;

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