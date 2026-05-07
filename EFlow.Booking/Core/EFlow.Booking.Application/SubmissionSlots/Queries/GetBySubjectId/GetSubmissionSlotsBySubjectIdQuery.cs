using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetSubmissionSlotsBySubjectIdQuery : IRequest<Result<IEnumerable<SubmissionSlotView>>>, ICacheableRequest
{
    public required Guid SubjectId { get; init; }
}