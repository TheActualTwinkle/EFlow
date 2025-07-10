using FluentResults;
using MediatR;

namespace EFlow.Application.Teachers.Queries;

public record GetTeacherByIdQuery : IRequest<Result<TeacherDto>>
{
    public required Guid Id { get; init; }
}