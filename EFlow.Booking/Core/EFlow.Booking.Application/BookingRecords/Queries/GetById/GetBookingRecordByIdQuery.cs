using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public record GetBookingRecordByIdQuery : IRequest<Result<BookingRecordDto>>
{
    public required Guid Id { get; init; }
}