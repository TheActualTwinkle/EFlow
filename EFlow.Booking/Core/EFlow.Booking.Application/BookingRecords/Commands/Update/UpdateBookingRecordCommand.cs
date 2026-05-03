using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.BookingRecords.Commands.Update;

public record UpdateBookingRecordCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }

    public Guid? StudentId { get; init; }

    public Guid? SlotId { get; init; }
}