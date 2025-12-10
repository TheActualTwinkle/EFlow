using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Queries;

public record GetStudentByIdQuery : IRequest<Result<StudentDto>>
{
    public required Guid Id { get; init; }
}