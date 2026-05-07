using EFlow.Booking.Contracts.SubmissionSlots;
using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public record GetAllSubmissionSlotsQuery : IRequest<Result<IEnumerable<SubmissionSlotView>>>;