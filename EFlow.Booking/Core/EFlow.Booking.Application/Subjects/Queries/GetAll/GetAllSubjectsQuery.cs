using EFlow.Booking.Contracts.Subjects;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Queries;

public record GetAllSubjectsQuery : IRequest<Result<IEnumerable<SubjectView>>>;