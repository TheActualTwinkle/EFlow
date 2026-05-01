using EFlow.Booking.Contracts.BookingRecords;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public record GetBookingRecordsByStudentIdQuery : IRequest<Result<IEnumerable<BookingRecordView>>>
{
    public required Guid StudentId { get; init; }
}