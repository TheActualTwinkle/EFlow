using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Bookings.Queries;

public class GetAllBookingsQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetAllBookingsQuery, Result<IEnumerable<BookingDto>>>
{
    public async Task<Result<IEnumerable<BookingDto>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
    {
        var bookings = (await unitOfWork
                .GetRepository<IBookingRepository>()
                .GetAllAsync(cancellationToken))
            .Adapt<IEnumerable<BookingDto>>();

        return Result.Ok(bookings);
    }
}