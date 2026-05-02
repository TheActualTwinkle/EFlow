using EFlow.Booking.Contracts.Students;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Queries;

public record GetAllStudentsQuery : IRequest<Result<IEnumerable<StudentView>>>;