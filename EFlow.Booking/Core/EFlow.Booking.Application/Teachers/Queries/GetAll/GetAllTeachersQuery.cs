using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Queries;

public record GetAllTeachersQuery : IRequest<Result<IEnumerable<TeacherDto>>>;