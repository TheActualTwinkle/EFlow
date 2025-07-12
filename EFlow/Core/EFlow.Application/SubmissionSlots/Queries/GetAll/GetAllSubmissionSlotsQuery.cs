using FluentResults;
using MediatR;

namespace EFlow.Application.SubmissionSlots.Queries;

public record GetAllSubmissionSlotsQuery : IRequest<Result<IEnumerable<SubmissionSlotDto>>>;