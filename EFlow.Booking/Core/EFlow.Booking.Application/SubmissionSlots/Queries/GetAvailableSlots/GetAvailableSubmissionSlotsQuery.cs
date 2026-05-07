using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetAvailableSubmissionSlotsQuery : IRequest<Result<IEnumerable<SubmissionSlotView>>>, ICacheableRequest
{
    public required DateTime FromDate { get; init; }
}