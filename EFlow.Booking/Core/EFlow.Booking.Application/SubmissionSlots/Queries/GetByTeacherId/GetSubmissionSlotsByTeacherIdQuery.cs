using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries.GetByTeacherId;

public sealed record GetSubmissionSlotsByTeacherIdQuery : IRequest<Result<IEnumerable<SubmissionSlotView>>>
{
    public required Guid TeacherId { get; init; }
}