using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetSubmissionSlotByIdQuery : IRequest<Result<SubmissionSlotView>>
{
    public required Guid Id { get; init; }
}