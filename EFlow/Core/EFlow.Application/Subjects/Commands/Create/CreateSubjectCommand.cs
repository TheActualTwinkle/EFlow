using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Subjects.Commands;

public record CreateSubjectCommand : IRequest<Result<Guid>>, ITransactionalRequest
{
    public required string Name { get; init; }

    public required Guid TeacherId { get; init; }
    
    public required ICollection<Guid> GroupIds { get; init; }
}