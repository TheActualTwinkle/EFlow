using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Application.Groups.Commands;

public class DeleteGroupCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteGroupCommand, Result>
{
    public async Task<Result> Handle(DeleteGroupCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork
            .GetRepository<IGroupRepository>()
            .DeleteAsync(request.Id, cancellationToken);

        return Result.Ok();
    }
}