using EFlow.Booking.Application.Common.Markers;
using EFlow.Booking.Domain.Subjects;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Commands.Update;

public record UpdateSubjectCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }

    public required SubjectUpdatePatch Patch { get; init; }
}
