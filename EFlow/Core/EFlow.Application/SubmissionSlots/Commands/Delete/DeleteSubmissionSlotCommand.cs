using EFlow.Application.Common.Markers;
using FluentResults;
using MediatR;

namespace EFlow.Application.SubmissionSlots.Commands;

public record DeleteSubmissionSlotCommand : IRequest<Result>, ITransactionalRequest
{
    public required Guid Id { get; init; }
}