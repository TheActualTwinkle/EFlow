using EFlow.Booking.Contracts.BookingRecords;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Queries;

public record GetBookingRecordByIdQuery : IRequest<Result<BookingRecordView>>
{
    public required Guid Id { get; init; }
}