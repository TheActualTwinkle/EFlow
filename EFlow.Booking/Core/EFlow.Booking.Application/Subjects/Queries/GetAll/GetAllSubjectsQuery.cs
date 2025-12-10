using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public record GetAllSubjectsQuery : IRequest<Result<IEnumerable<SubjectDto>>>;