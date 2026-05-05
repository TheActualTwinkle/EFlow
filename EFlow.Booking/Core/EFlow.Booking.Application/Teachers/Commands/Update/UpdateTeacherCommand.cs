using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Domain.Teachers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Teachers.Commands.Update;

public record UpdateTeacherCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }

    public required TeacherUpdatePatch Patch { get; init; }
}
