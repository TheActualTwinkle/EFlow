using EFlow.Booking.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.Subjects.Commands.Update;

public record UpdateSubjectCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }

    public string? Name { get; init; }

    public Guid? TeacherId { get; init; }
    
    public IEnumerable<Guid>? GroupIds { get; init; }
}