using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Commands;

public class DeleteBookingCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteBookingCommand, Result>
{
    public async Task<Result> Handle(DeleteBookingCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork
            .GetRepository<IBookingRepository>()
            .DeleteAsync(request.Id, cancellationToken);

        return Result.Ok();
    }
}