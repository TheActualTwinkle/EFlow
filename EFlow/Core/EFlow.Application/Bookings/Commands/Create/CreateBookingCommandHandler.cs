using EFlow.Domain;
using EFlow.Domain.Models;
using EFlow.Domain.Repositories;
using FluentResults;
using MediatR;

namespace EFlow.Application.Bookings.Commands;

public class CreateBookingCommandHandler(
    IUnitOfWork unitOfWork)
    : IRequestHandler<CreateBookingCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            StudentId = request.StudentId,
            SlotId = request.SlotId,
            CreatedAt = request.CreatedAt ?? DateTime.UtcNow
        };

        await unitOfWork
            .GetRepository<IBookingRepository>()
            .CreateAsync(booking, cancellationToken);

        return Result.Ok(booking.Id);
    }
}