using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Commands;

public record AddAdmissionCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required Guid SlotId { get; init; }

    public required Guid StudentId { get; init; }
}
