using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands;

public record CancelBookingCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}