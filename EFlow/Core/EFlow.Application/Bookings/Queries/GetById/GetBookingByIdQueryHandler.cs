using EFlow.Application.Common.Errors.Abstractions;
using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Bookings.Queries;

public class GetBookingByIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingByIdQuery, Result<BookingDto>>
{
    public async Task<Result<BookingDto>> Handle(GetBookingByIdQuery request, CancellationToken cancellationToken)
    {
        var booking = await unitOfWork
            .GetRepository<IBookingRepository>()
            .GetByIdAsync(request.Id, cancellationToken);

        if (booking is null)
            return Result.Fail(
                new NotFoundError()
                    .WithMessage("Booking not found")
                    .WithId(request.Id));

        return booking.Adapt<BookingDto>();
    }
}