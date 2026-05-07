using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetSubmissionSlotByIdQuery : IRequest<Result<SubmissionSlotView>>, ICacheableRequest
{
    public required Guid Id { get; init; }
}