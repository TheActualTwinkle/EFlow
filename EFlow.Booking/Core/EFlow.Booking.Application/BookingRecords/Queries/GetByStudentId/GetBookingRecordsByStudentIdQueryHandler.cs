using EFlow.Booking.Contracts.BookingRecords;
using EFlow.Booking.Domain.Students;
using EFlow.Common.Infrastructure;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public class GetBookingRecordsByStudentIdQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBookingRecordsByStudentIdQuery, Result<IEnumerable<BookingRecordView>>>
{
    public async Task<Result<IEnumerable<BookingRecordView>>> Handle(GetBookingRecordsByStudentIdQuery request, CancellationToken cancellationToken)
    {
        var bookings = (await unitOfWork
            .GetQueryService<IBookingRecordQueryService>()
            .GetByStudentIdAsync(new StudentId(request.StudentId), cancellationToken));

        return Result.Ok(bookings);
    }
}