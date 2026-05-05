using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Domain.Students;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Students.Commands.Update;

public record UpdateStudentCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }

    public required StudentUpdatePatch Patch { get; init; }
}
