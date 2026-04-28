using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Commands;

public record DeleteStudentCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}