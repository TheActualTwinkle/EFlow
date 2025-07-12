using FluentResults;
using MediatR;

namespace EFlow.Application.Subjects.Queries;

public record GetSubjectByIdQuery : IRequest<Result<SubjectDto>>
{
    public required Guid Id { get; init; }
}