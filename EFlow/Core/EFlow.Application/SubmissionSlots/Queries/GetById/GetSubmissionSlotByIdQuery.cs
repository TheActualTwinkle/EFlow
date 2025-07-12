using FluentResults;
using MediatR;

namespace EFlow.Application.SubmissionSlots.Queries;

public record GetSubmissionSlotByIdQuery : IRequest<Result<SubmissionSlotDto>>
{
    public required Guid Id { get; init; }
}