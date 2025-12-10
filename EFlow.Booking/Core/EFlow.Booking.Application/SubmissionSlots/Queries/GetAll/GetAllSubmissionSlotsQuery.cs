using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetAllSubmissionSlotsQuery : IRequest<Result<IEnumerable<SubmissionSlotDto>>>;