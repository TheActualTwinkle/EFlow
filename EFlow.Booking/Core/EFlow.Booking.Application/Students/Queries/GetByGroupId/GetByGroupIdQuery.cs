using EFlow.Booking.Contracts.Students;
using EFlow.Booking.Domain.Groups;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Queries;

public sealed record GetByGroupIdQuery : IRequest<Result<IEnumerable<StudentView>>>
{
    public required GroupId GroupId { get; init; }
}