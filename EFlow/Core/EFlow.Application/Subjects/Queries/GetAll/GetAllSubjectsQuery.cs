using FluentResults;
using MediatR;

namespace EFlow.Application.Subjects.Queries;

public record GetAllSubjectsQuery : IRequest<Result<IEnumerable<SubjectDto>>>;