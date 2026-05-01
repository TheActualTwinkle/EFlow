using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetAvailableSubmissionSlotsQuery : IRequest<Result<IEnumerable<SubmissionSlotView>>>
{
    public required DateTime FromDate { get; init; }
}