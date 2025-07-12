using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.Subjects.Commands;

public record DeleteSubjectCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}