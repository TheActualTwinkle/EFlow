using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public record GetSubjectsByTeacherIdQuery : IRequest<Result<IEnumerable<SubjectDto>>>
{
    public required Guid TeacherId { get; init; }
}