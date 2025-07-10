using FluentResults;
using MediatR;

namespace EFlow.Application.Teachers.Queries;

public record GetAllTeachersQuery : IRequest<Result<IEnumerable<TeacherDto>>>;