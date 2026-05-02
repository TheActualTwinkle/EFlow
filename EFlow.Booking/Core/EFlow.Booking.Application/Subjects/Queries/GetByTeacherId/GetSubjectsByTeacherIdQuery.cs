using EFlow.Booking.Contracts.Subjects;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public record GetSubjectsByTeacherIdQuery : IRequest<Result<IEnumerable<SubjectView>>>
{
    public required Guid TeacherId { get; init; }
}