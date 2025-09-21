using EFlow.Application.Common.Errors;
using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Groups.Commands.Update;

public class UpdateGroupCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateGroupCommand, Result>
{
    public async Task<Result> Handle(UpdateGroupCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IGroupRepository>();

        var group = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (group is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Group not found")
                    .WithId(request.Id));

        request.Adapt(group);

        repository.Update(group);

        return Result.Ok();
    }
}