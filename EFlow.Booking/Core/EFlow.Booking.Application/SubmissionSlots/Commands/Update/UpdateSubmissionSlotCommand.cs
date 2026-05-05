using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Domain.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands.Update;

public record UpdateSubmissionSlotCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }

    public required SubmissionSlotPatch Patch { get; init; }
}
