using FluentResults;
using MediatR;

namespace EFlow.Application.Students.Queries;

public record GetAllStudentsQuery : IRequest<Result<IEnumerable<StudentDto>>>;