using EFlow.Application.Common.Errors;
using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Bookings.Commands.Update;

public class UpdateBookingCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateBookingCommand, Result>
{
    public async Task<Result> Handle(UpdateBookingCommand request, CancellationToken cancellationToken)
    {
        var repository = unitOfWork.GetRepository<IBookingRepository>();

        var booking = await repository.GetByIdAsync(request.Id, cancellationToken);

        if (booking is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Booking not found")
                    .WithId(request.Id));

        request.Adapt(booking);

        repository.Update(booking);

        return Result.Ok();
    }
}