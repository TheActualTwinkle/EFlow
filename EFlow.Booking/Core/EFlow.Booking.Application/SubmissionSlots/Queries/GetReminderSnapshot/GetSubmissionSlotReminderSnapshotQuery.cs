using FluentResults;
using MediatR;

namespace EFlow.Booking.Application.SubmissionSlots.Queries;

public sealed record GetSubmissionSlotReminderSnapshotQuery
    : IRequest<Result<IEnumerable<SubmissionSlotReminderSnapshotDto>>>;
