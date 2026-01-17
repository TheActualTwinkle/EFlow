using EFlow.Common.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Groups.Commands;

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