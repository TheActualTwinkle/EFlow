using EFlow.Booking.Domain;
using EFlow.Common.Infrastructure;
using FluentResults;
using Mapster;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public class GetBookingRecordsByStudentIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingRecordsByStudentIdQuery, Result<IEnumerable<BookingRecordDto>>>
{
    public async Task<Result<IEnumerable<BookingRecordDto>>> Handle(GetBookingRecordsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = (await unitOfWork
                .GetRepository<IBookingRecordRepository>()
                .GetByStudentIdAsync(request.StudentId, cancellationToken))
            .Adapt<IEnumerable<BookingRecordDto>>();

        return Result.Ok(bookings);
    }
}