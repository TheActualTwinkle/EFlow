using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public record GetBookingRecordsByStudentIdQuery : IRequest<Result<IEnumerable<BookingRecordDto>>>
{
    public required Guid StudentId { get; init; }
}