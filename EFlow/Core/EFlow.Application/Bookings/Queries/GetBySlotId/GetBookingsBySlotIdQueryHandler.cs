using EFlow.Domain;
using EFlow.Domain.Repositories;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Application.Bookings.Queries;

public class GetBookingsBySlotIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingsBySlotIdQuery, Result<IEnumerable<BookingDto>>>
{
    public async Task<Result<IEnumerable<BookingDto>>> Handle(GetBookingsBySlotIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = (await unitOfWork
                .GetRepository<IBookingRepository>()
                .GetBySlotIdAsync(request.SlotId, cancellationToken))
            .Adapt<IEnumerable<BookingDto>>();

        return Result.Ok(bookings);
    }
}