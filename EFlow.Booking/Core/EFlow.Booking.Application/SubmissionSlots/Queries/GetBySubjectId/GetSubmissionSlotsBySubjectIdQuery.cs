using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetSubmissionSlotsBySubjectIdQuery : IRequest<Result<IEnumerable<SubmissionSlotView>>>
{
    public required Guid SubjectId { get; init; }
}