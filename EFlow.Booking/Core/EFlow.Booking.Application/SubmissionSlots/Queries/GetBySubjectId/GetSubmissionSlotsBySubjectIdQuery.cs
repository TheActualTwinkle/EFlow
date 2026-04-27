using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetSubmissionSlotsBySubjectIdQuery : IRequest<Result<IEnumerable<SubmissionSlotDto>>>
{
    public required Guid SubjectId { get; init; }
}