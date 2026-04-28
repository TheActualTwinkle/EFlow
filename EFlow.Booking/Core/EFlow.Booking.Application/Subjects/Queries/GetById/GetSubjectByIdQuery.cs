using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public record GetSubjectByIdQuery : IRequest<Result<SubjectDto>>
{
    public required Guid Id { get; init; }
}