using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetAvailableSubmissionSlotsQuery : IRequest<Result<IEnumerable<SubmissionSlotDto>>>
{
    public required DateTime FromDate { get; init; }
}