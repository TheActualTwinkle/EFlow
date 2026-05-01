using EFlow.Booking.Contracts.Students;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Queries;

public record GetStudentByIdQuery : IRequest<Result<StudentView>>
{
    public required Guid Id { get; init; }
}