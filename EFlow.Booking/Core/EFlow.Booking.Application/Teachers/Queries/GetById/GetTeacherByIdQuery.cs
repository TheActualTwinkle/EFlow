using EFlow.Booking.Contracts.Teachers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Queries;

public record GetTeacherByIdQuery : IRequest<Result<TeacherView>>
{
    public required Guid Id { get; init; }
}